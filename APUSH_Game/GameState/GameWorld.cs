using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APUSH_Game.Helpers;
using Newtonsoft.Json;
using System.Diagnostics;
using APUSH_Game.Interface;

namespace APUSH_Game.GameState
{
    internal class GameWorld
    {
        private readonly RegionObject[] _regions;
        public readonly List<TerritoryQuestion> questions;

        private readonly GameCamera gameCamera;
        public GameCamera Camera => gameCamera;

        private GameGUI _union;
        private GameGUI _confederacy;
        public bool Current { get; set; }
        private bool _activeTransition = false;
        private GameGUI _currentRef;
        public GameGUI CurrentGameGUI => _currentRef;

        private readonly Texture2D _bg;
        private readonly Texture2D _pk;
        private readonly Color[] colors;
        public TurnAction SelectedAction => _currentRef.CurrentAction;
        public readonly List<IGameObject> GameObjects = new List<IGameObject>();

        public bool ScrollLock { get; set; } = false;
        public StateManager State { get; private init; }

        private List<PromptBox> questionsUpdate = new();

        public GameWorld()
        {
            gameCamera = new GameCamera();
            gameCamera.Location = new Vector2(3150.7664f,  1723.9828f);
            gameCamera.Zoom = 0.284573227f;
            _bg = GameRoot.Game.Content.Load<Texture2D>("mapBg");

            _pk = GameRoot.Game.Content.Load<Texture2D>("Packed");
            colors = new Color[_pk.Width * _pk.Height];
            _pk.GetData(colors);

            var data = JsonConvert.DeserializeObject<Region[]>(File.ReadAllText("terr.json"));
            questions = JsonConvert.DeserializeObject<List<TerritoryQuestion>>(File.ReadAllText("ques.json"));

            _regions = new RegionObject[data.Length];
            for(int i = 0; i < data.Length; i++)
            {
                GameObjects.Add(_regions[i] = new RegionObject(data[i], this, i, Random.Shared.Next(2) + 1));
            }
            Set("Washington D.C", 5, 7);
            Set("Missouri", 5, 7);
            Set("West Maryland", 4, 6);
            Set("California", 4, 6);

            Set("New Orleans", 6, 8);
            Set("Richmond", 5, 7);
            Set("Shiloh", 4, 6);

            void Set(string name, int min, int? max = null)
            {
                for(int i = 0; i < _regions.Length; i++)
                {
                    if (_regions[i].Data.RegionName == name)
                    {
                        _regions[i].CurrentTroops.TroopCount = Random.Shared.Next(min, max ?? (min + 1));
                        break;
                    }
                }
            }
            State = new StateManager(_regions, this);
            _currentRef = _union = new GameGUI(true);
            _confederacy = new GameGUI(false);
            Current = true;
        }

        private Vector2 cameraVelocity = Vector2.Zero;

        private float zoomMValue = 1;

        const float accelerationRate = 0.3f;
        const float decelerationRate = 0.98f;

        private int turn;

        private bool SupressLoss;
        public void Tick(GameTime Gametime)
        {
            if((!SupressLoss && UnionCount == 0 || SouthCount == 0) || turn > 20)
            {
                SupressLoss = true;
                AnimationPool.Instance.Request().Reset(f =>
                {
                    GameRoot.Game.PostDraw += g =>
                    {
                        Globals.SpriteBatch.Draw(Globals.Pixel, new Rectangle(Point.Zero, Globals.WindowSize), Color.Black * f);
                    };
                }, 0, () => ScreenManager.Instance.ChangeState(new EndState(UnionCount == 0, turn)), new KeyFrame(1, 120, AnimationType.EaseInOutQuart));
            }
            if(SupressLoss)
            {
                return;
            }
            gameCamera.Update(Gametime);
            for(int i = 0; i < questionsUpdate.Count; i++)
                questionsUpdate[i].Update();

            if (question)
            {
                return;
            }

            #region Camera
            Vector2 accel = Vector2.Zero;
            if (InputHelper.Down(Keys.W))
                accel += new Vector2(0, -accelerationRate);
            if (InputHelper.Down(Keys.S))
                accel += new Vector2(0, accelerationRate);
            if (InputHelper.Down(Keys.A))
                accel += new Vector2(-accelerationRate, 0);
            if (InputHelper.Down(Keys.D))
                accel += new Vector2(accelerationRate, 0);
            if (InputHelper.DeltaScroll != 0 && !ScrollLock)
                zoomMValue = InputHelper.DeltaScroll > 0 ? 1.05f : 0.95f;

            gameCamera.Zoom *= zoomMValue;
            //2.9, .25: range, 0.2 smooth const
            if (gameCamera.Zoom > 2.9f)
                gameCamera.Zoom -= (gameCamera.Zoom - 2.9f) * 0.2f;
            if (gameCamera.Zoom < 0.25f)
                gameCamera.Zoom -= (gameCamera.Zoom - 0.25f) * 0.2f;

            zoomMValue += (1 - zoomMValue) * 0.1f;

            cameraVelocity += accel;
            cameraVelocity *= decelerationRate;

            if (InputHelper.Down(MouseButton.Left))
                gameCamera.Location += (InputHelper.PrevMouseState.Position.V() - InputHelper.MouseLocation.V()) / gameCamera.Zoom;

            gameCamera.Location += cameraVelocity / gameCamera.Zoom;

            Vector2 fromCenter = new Vector2(3150.7664f, 1723.9828f) - gameCamera.Location;
            if (Math.Abs(fromCenter.X) > 2000)
            {
                var clx = gameCamera.Location.X;
                gameCamera.Location = new Vector2(clx.Approach(3150, 0.01f), gameCamera.Location.Y);
            }
            if (Math.Abs(fromCenter.Y) > 2000)
            {
                var cly = gameCamera.Location.Y;
                gameCamera.Location = new Vector2(gameCamera.Location.X, cly.Approach(1723.9828f, 0.01f));
            }
            #endregion

            for (int i = GameObjects.Count - 1; i >= 0; i--)
            {
                GameObjects[i].Update();
                if (GameObjects[i].Delete)
                {
                    GameObjects.RemoveAt(i);
                }
            }
            _currentRef.Tick(Gametime);

            if((_currentRef.NumPoliticalCapital <= 0 || InputHelper.RisingEdge(Keys.Space))&& !_activeTransition)
            {
                turn++;
                _confederacy.Turn.Text = _union.Turn.Text = turn.ToString();
                if(_currentRef.NumPoliticalCapital == 0)
                    GameObjects.Add(new CursorMessage($"Not enough Political Capital. Continuing..."));

                _activeTransition = true;
                _currentRef.AnimateOut().SetOnEnd(()=>
                {
                    Current = !Current;
                    _currentRef = _currentRef == _union ? _confederacy : _union;
                    NextPlayer();
                    _currentRef.AnimateIn().SetOnEnd(() => _activeTransition = false);
                });
            }

            State.Update();
        }

        private void NextPlayer()
        {
            float p;
            int politicalCapitalIncrement;
            int dollarsIncrement;

            if (Current)
            {
                p = UnionCount / (float)GameGUI.OriginalUnionTerritories;
                politicalCapitalIncrement = (int)Math.Clamp(2 * p, 1, 5);
                dollarsIncrement = (int)Math.Clamp(4 * p, 1, 5);
            }
            else
            {
                p = SouthCount / (float)GameGUI.OriginalConfederateTerritories;
                politicalCapitalIncrement = (int)Math.Clamp(3 * p, 1, 5);
                dollarsIncrement = (int)Math.Clamp(2 * p, 1, 5);
            }

            _currentRef.NumPoliticalCapital += politicalCapitalIncrement;
            _currentRef.NumDollars += dollarsIncrement;

            AnimationPool.Instance.Request().Reset(Actions.Empty, 0, () => GameObjects.Add(new CursorMessage($"+{dollarsIncrement} Dollars. +{politicalCapitalIncrement} Political Captiol")), new KeyFrame(0, 60));
        }


        private static T Route<T>(T input, Action<T> toDo)
        {
            toDo(input);
            return input;
        }

        private int SouthCount => _regions.Count(c => c.Data.TerritoryType == TerrioryType.ConfederateState);
        private int UnionCount => 96 - SouthCount;
        private static readonly Color BGColor = new Color(0, 162, 232);
        public void Draw(GameTime Gametime)
        {
            Globals.Graphics.Clear(BGColor);
            
            gameCamera.StartSpriteBatch(Globals.SpriteBatch);
            Globals.SpriteBatch.Draw(_bg, Vector2.Zero, Color.White);
            foreach (var item in questionsUpdate)
                item.Draw();
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].Draw();
            }
            State.Draw();
            Globals.SpriteBatch.End();
            
            Globals.SpriteBatch.Begin();
            _currentRef.Draw(Gametime);
            GameRoot.Game.PostDraw?.Invoke(Gametime);
            GameRoot.Game.PostDraw = null;
            Globals.SpriteBatch.End();
        }

        public Color GetColorAtLoc(Point p)
        {
            if((uint)p.X > (uint)_pk.Width || (uint)p.Y > (uint)_pk.Height)
                return Color.Transparent;

            return colors[p.X + p.Y * _pk.Width];
        }

        private bool question = false;
        public void AskQuestion(TerritoryQuestion q)
        {
            question = true;
            string correct = q.Answers[0];
            for (int i = 2; i >= 0; i--)
            {
                int newIndex = Random.Shared.Next(i);
                (q.Answers[i], q.Answers[newIndex]) = (q.Answers[newIndex], q.Answers[i]);
            }
            int correctIndex = Array.IndexOf(q.Answers, correct);

            var b = new PromptBox(RebuildSentence(q.Question), new Vector2(0.5f, 0.35f), false);
            questionsUpdate.Add(b);
            Vector2 hoverBoxBelow = new Vector2(b.PromptBounds.Center.X, b.PromptBounds.Bottom + 128 * Globals.ScaleVec.Y) / Globals.WindowSize.V();
            Vector2 ansLeftPos = hoverBoxBelow + new Vector2(0.25f, 0);
            Vector2 ansRightPos = hoverBoxBelow - new Vector2(0.25f, 0);

            questionsUpdate.Add(new PromptBox(q.Answers[1], hoverBoxBelow, true, () => AnswerClicked(1), 0.7f));
            questionsUpdate.Add(new PromptBox(q.Answers[0], ansLeftPos, true, () => AnswerClicked(0), 0.7f));
            questionsUpdate.Add(new PromptBox(q.Answers[2], ansRightPos, true, () => AnswerClicked(2), 0.7f));

            void AnswerClicked(int index)
            {
                if(correctIndex == index)
                {
                    _currentRef.NumDollars += 2;
                    GameObjects.Add(new CursorMessage("+2 Dollars"));
                }
                questionsUpdate.Clear();
                AnimationPool.Instance.Request().Reset(Actions.Empty, 0, () => question = false, new KeyFrame(0, 60));
            }
        }

        public static string RebuildSentence(string s)
        {
            StringBuilder sb = new StringBuilder();
            string[] words = s.Split(' ');
            float travel = 0;

            for (int i = 0; i < words.Length; i++)
            {
                sb.Append(words[i]);
                if(travel > 1600)
                {
                    sb.Append('\n');
                    travel = 0;
                }
                else
                {
                    sb.Append(' ');
                }

                travel += Globals.Font.MeasureString(words[i]).X;
            }

            return sb.ToString();
        }
    }
}
