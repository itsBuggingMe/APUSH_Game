﻿using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APUSH_Game.Helpers;
using Newtonsoft.Json;
using System.Diagnostics;
using APUSH_Game.Interface;
using WebPort;

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

        private readonly LargeTexture _bg;
        private readonly LargeTexture _pk;
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
            _bg = new LargeTexture("mapBg");

            _pk = new LargeTexture("mapBg");

            var data = JsonConvert.DeserializeObject<Region[]>(JSON.Terr);
            questions = JsonConvert.DeserializeObject<List<TerritoryQuestion>>(JSON.Ques);

            _regions = new RegionObject[data.Length];
            for(int i = 0; i < data.Length; i++)
            {
                GameObjects.Add(_regions[i] = new RegionObject(data[i], this, i, Random.Shared.Next(2) + 1));
            }
            Set("Washington D.C", 5, 7);
            Set("Missouri", 5, 7);
            Set("West Maryland", 4, 6);
            Set("California", 4, 6);

            Set("New Orleans", 8, 10);
            Set("Richmond", 5, 7);
            Set("Shiloh", 4, 6);
            Set("Atlanta", 4, 6);
            Set("Savannah", 4, 6);

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
            if(Globals.TotalFrames % 60 == 0 && !SupressLoss && (UnionCount == 0 || SouthCount == 0 || turn > 2))
            {
                SupressLoss = true;
                GameObjects.RemoveAll(g => g is CursorMessage);
                FadeTransition(new EndState(UnionCount == 0, turn));
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
            if(InputHelper.Down(Keys.Q))
                zoomMValue = 1.05f;
            if (InputHelper.Down(Keys.E))
                zoomMValue = 0.95f;
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

        public static void FadeTransition(IScreen newState)
        {
            AnimationPool.Instance.Request().Reset(f =>
            {
                GameRoot.Game.PostDraw += g =>
                {
                    Globals.SpriteBatch.Draw(Globals.Pixel, new Rectangle(Point.Zero, Globals.WindowSize), Color.Black * f);
                };
            }, 0, () =>
            {
                ScreenManager.Instance.ChangeState(newState);
                AnimationPool.Instance.Request().Reset(f =>
                {
                    GameRoot.Game.PostDraw += g =>
                    {
                        Globals.SpriteBatch.Draw(Globals.Pixel, new Rectangle(Point.Zero, Globals.WindowSize), Color.Black * f);
                    };
                }, 1, null, new KeyFrame(0, 120, AnimationType.EaseInOutQuart));
            }, new KeyFrame(1, 120, AnimationType.EaseInOutQuart));
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
            Globals.SpriteBatch.DrawLarge(_bg, Color.White);
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
            return _pk.GetColorAtPosition(p);
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

    public static class JSON
    {
        public const string Terr = "[{\"TextureSource\":{\"X\":0,\"Y\":0,\"Width\":1501,\"Height\":1179},\"MapBounds\":{\"X\":1862,\"Y\":371,\"Width\":1501,\"Height\":1179},\"CenterX\":709.6001,\"CenterY\":619.4999,\"TerritoryType\":0,\"ID\":0,\"RegionName\":\"Nebraska Territory\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1196,\"Y\":1179,\"Width\":1294,\"Height\":802},\"MapBounds\":{\"X\":1379,\"Y\":1683,\"Width\":1294,\"Height\":802},\"CenterX\":1903.0266,\"CenterY\":1586.2081,\"TerritoryType\":0,\"ID\":1,\"RegionName\":\"New Mexico Territory\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1501,\"Y\":0,\"Width\":1278,\"Height\":790},\"MapBounds\":{\"X\":1185,\"Y\":1058,\"Width\":1278,\"Height\":790},\"CenterX\":2114.994,\"CenterY\":400.09772,\"TerritoryType\":0,\"ID\":2,\"RegionName\":\"Utah Territory\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":0,\"Y\":1179,\"Width\":1196,\"Height\":1038},\"MapBounds\":{\"X\":1078,\"Y\":211,\"Width\":1196,\"Height\":1038},\"CenterX\":590.3742,\"CenterY\":1659.3328,\"TerritoryType\":0,\"ID\":3,\"RegionName\":\"Washington Territory\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1196,\"Y\":1981,\"Width\":741,\"Height\":1262},\"MapBounds\":{\"X\":852,\"Y\":960,\"Width\":741,\"Height\":1262},\"CenterX\":1517.6384,\"CenterY\":2630.2507,\"TerritoryType\":1,\"ID\":4,\"RegionName\":\"California\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1937,\"Y\":1981,\"Width\":1153,\"Height\":396},\"MapBounds\":{\"X\":2284,\"Y\":1505,\"Width\":1153,\"Height\":396},\"CenterX\":2592.6182,\"CenterY\":2177.2703,\"TerritoryType\":0,\"ID\":5,\"RegionName\":\"Kansas Territory\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":0,\"Y\":2217,\"Width\":743,\"Height\":632},\"MapBounds\":{\"X\":916,\"Y\":484,\"Width\":743,\"Height\":632},\"CenterX\":363.57126,\"CenterY\":2555.7092,\"TerritoryType\":0,\"ID\":6,\"RegionName\":\"Oregon\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1937,\"Y\":2377,\"Width\":601,\"Height\":683},\"MapBounds\":{\"X\":3213,\"Y\":459,\"Width\":601,\"Height\":683},\"CenterX\":2165.8335,\"CenterY\":2758.001,\"TerritoryType\":1,\"ID\":7,\"RegionName\":\"Minnesota\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2490,\"Y\":1179,\"Width\":492,\"Height\":774},\"MapBounds\":{\"X\":2781,\"Y\":484,\"Width\":492,\"Height\":774},\"CenterX\":2783.2988,\"CenterY\":1480.0992,\"TerritoryType\":0,\"ID\":8,\"RegionName\":\"Unorganized Dakota Territory\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2779,\"Y\":0,\"Width\":511,\"Height\":634},\"MapBounds\":{\"X\":2615,\"Y\":2134,\"Width\":511,\"Height\":634},\"CenterX\":2988.474,\"CenterY\":288.81122,\"TerritoryType\":2,\"ID\":9,\"RegionName\":\"Central Texas\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":607,\"Y\":2849,\"Width\":556,\"Height\":519},\"MapBounds\":{\"X\":3000,\"Y\":2252,\"Width\":556,\"Height\":519},\"CenterX\":904.7763,\"CenterY\":3089.7517,\"TerritoryType\":2,\"ID\":10,\"RegionName\":\"Eastern Texas\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":0,\"Y\":2849,\"Width\":607,\"Height\":531},\"MapBounds\":{\"X\":3335,\"Y\":1472,\"Width\":607,\"Height\":531},\"CenterX\":289.59454,\"CenterY\":3110.4626,\"TerritoryType\":1,\"ID\":11,\"RegionName\":\"Missouri\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2538,\"Y\":2377,\"Width\":626,\"Height\":622},\"MapBounds\":{\"X\":2799,\"Y\":2573,\"Width\":626,\"Height\":622},\"CenterX\":2819.5728,\"CenterY\":2647.2927,\"TerritoryType\":2,\"ID\":12,\"RegionName\":\"Southern Texas\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2538,\"Y\":2999,\"Width\":529,\"Height\":403},\"MapBounds\":{\"X\":2928,\"Y\":1892,\"Width\":529,\"Height\":403},\"CenterX\":2807.2395,\"CenterY\":3180.4568,\"TerritoryType\":0,\"ID\":13,\"RegionName\":\"Indian Territory\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1501,\"Y\":790,\"Width\":560,\"Height\":366},\"MapBounds\":{\"X\":3255,\"Y\":1132,\"Width\":560,\"Height\":366},\"CenterX\":1767.0387,\"CenterY\":960.0909,\"TerritoryType\":1,\"ID\":14,\"RegionName\":\"Iowa\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2779,\"Y\":634,\"Width\":487,\"Height\":530},\"MapBounds\":{\"X\":3560,\"Y\":713,\"Width\":487,\"Height\":530},\"CenterX\":3018.3462,\"CenterY\":909.98236,\"TerritoryType\":1,\"ID\":15,\"RegionName\":\"Wisconsin\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2982,\"Y\":1179,\"Width\":365,\"Height\":648},\"MapBounds\":{\"X\":3703,\"Y\":1228,\"Width\":365,\"Height\":648},\"CenterX\":3182.6316,\"CenterY\":1471.0875,\"TerritoryType\":1,\"ID\":16,\"RegionName\":\"Illinois\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1937,\"Y\":3060,\"Width\":459,\"Height\":418},\"MapBounds\":{\"X\":3438,\"Y\":1945,\"Width\":459,\"Height\":418},\"CenterX\":2139.8145,\"CenterY\":3252.274,\"TerritoryType\":2,\"ID\":17,\"RegionName\":\"Arkansas\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3067,\"Y\":2999,\"Width\":361,\"Height\":487},\"MapBounds\":{\"X\":4094,\"Y\":821,\"Width\":361,\"Height\":487},\"CenterX\":3230.059,\"CenterY\":3261.5002,\"TerritoryType\":1,\"ID\":18,\"RegionName\":\"Eastern Michigan\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3164,\"Y\":2377,\"Width\":387,\"Height\":439},\"MapBounds\":{\"X\":4269,\"Y\":1211,\"Width\":387,\"Height\":439},\"CenterX\":3354.9539,\"CenterY\":2596.695,\"TerritoryType\":1,\"ID\":19,\"RegionName\":\"Ohio\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3290,\"Y\":0,\"Width\":282,\"Height\":490},\"MapBounds\":{\"X\":4026,\"Y\":1290,\"Width\":282,\"Height\":490},\"CenterX\":3423.6313,\"CenterY\":224.57802,\"TerritoryType\":1,\"ID\":20,\"RegionName\":\"Indiana\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1196,\"Y\":3243,\"Width\":330,\"Height\":477},\"MapBounds\":{\"X\":4652,\"Y\":2680,\"Width\":330,\"Height\":477},\"CenterX\":1366.0955,\"CenterY\":3465.0671,\"TerritoryType\":2,\"ID\":21,\"RegionName\":\"South Florida\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":743,\"Y\":2217,\"Width\":347,\"Height\":534},\"MapBounds\":{\"X\":5318,\"Y\":377,\"Width\":347,\"Height\":534},\"CenterX\":881.1639,\"CenterY\":2449.857,\"TerritoryType\":1,\"ID\":22,\"RegionName\":\"Maine\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1526,\"Y\":3243,\"Width\":224,\"Height\":571},\"MapBounds\":{\"X\":3843,\"Y\":2101,\"Width\":224,\"Height\":571},\"CenterX\":1644.078,\"CenterY\":3513.8306,\"TerritoryType\":2,\"ID\":23,\"RegionName\":\"Eastern Mississippi\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3347,\"Y\":1179,\"Width\":300,\"Height\":401},\"MapBounds\":{\"X\":3499,\"Y\":2356,\"Width\":300,\"Height\":401},\"CenterX\":3485.5637,\"CenterY\":1354.8787,\"TerritoryType\":2,\"ID\":24,\"RegionName\":\"Western Louisiana\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3290,\"Y\":872,\"Width\":422,\"Height\":303},\"MapBounds\":{\"X\":4347,\"Y\":2268,\"Width\":422,\"Height\":303},\"CenterX\":3482.5444,\"CenterY\":1040.6265,\"TerritoryType\":2,\"ID\":25,\"RegionName\":\"South Georgia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3290,\"Y\":490,\"Width\":382,\"Height\":382},\"MapBounds\":{\"X\":2268,\"Y\":2426,\"Width\":382,\"Height\":382},\"CenterX\":3519.7078,\"CenterY\":649.7426,\"TerritoryType\":2,\"ID\":26,\"RegionName\":\"Western Texas\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2538,\"Y\":3402,\"Width\":290,\"Height\":386},\"MapBounds\":{\"X\":4928,\"Y\":745,\"Width\":290,\"Height\":386},\"CenterX\":2690.6506,\"CenterY\":3591.9326,\"TerritoryType\":1,\"ID\":27,\"RegionName\":\"Northeastern New York\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2061,\"Y\":957,\"Width\":600,\"Height\":217},\"MapBounds\":{\"X\":4025,\"Y\":1846,\"Width\":600,\"Height\":217},\"CenterX\":2322.8245,\"CenterY\":1035.9872,\"TerritoryType\":2,\"ID\":28,\"RegionName\":\"Northern Tennessee\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":607,\"Y\":3368,\"Width\":450,\"Height\":235},\"MapBounds\":{\"X\":4709,\"Y\":1881,\"Width\":450,\"Height\":235},\"CenterX\":828.9687,\"CenterY\":3470.0798,\"TerritoryType\":2,\"ID\":29,\"RegionName\":\"Southern North Carolina\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":350,\"Y\":3380,\"Width\":226,\"Height\":369},\"MapBounds\":{\"X\":4042,\"Y\":2283,\"Width\":226,\"Height\":369},\"CenterX\":445.6434,\"CenterY\":3538.8142,\"TerritoryType\":2,\"ID\":30,\"RegionName\":\"Southwest Alabama\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3572,\"Y\":0,\"Width\":293,\"Height\":242},\"MapBounds\":{\"X\":2644,\"Y\":1935,\"Width\":293,\"Height\":242},\"CenterX\":3718.6,\"CenterY\":112.91194,\"TerritoryType\":2,\"ID\":31,\"RegionName\":\"Texas Panhandle\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3572,\"Y\":242,\"Width\":301,\"Height\":220},\"MapBounds\":{\"X\":4044,\"Y\":2082,\"Width\":301,\"Height\":220},\"CenterX\":3710.7412,\"CenterY\":352.0801,\"TerritoryType\":2,\"ID\":32,\"RegionName\":\"Northern Alabama\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2061,\"Y\":790,\"Width\":688,\"Height\":167},\"MapBounds\":{\"X\":4460,\"Y\":1703,\"Width\":688,\"Height\":167},\"CenterX\":2392.6624,\"CenterY\":861.30774,\"TerritoryType\":2,\"ID\":33,\"RegionName\":\"Southern Virginia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3428,\"Y\":2999,\"Width\":433,\"Height\":212},\"MapBounds\":{\"X\":4147,\"Y\":1590,\"Width\":433,\"Height\":212},\"CenterX\":3636.0823,\"CenterY\":3118.1453,\"TerritoryType\":1,\"ID\":34,\"RegionName\":\"Northern Kentucky\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1937,\"Y\":3478,\"Width\":413,\"Height\":221},\"MapBounds\":{\"X\":4411,\"Y\":2514,\"Width\":413,\"Height\":221},\"CenterX\":2154.6938,\"CenterY\":3584.4175,\"TerritoryType\":2,\"ID\":35,\"RegionName\":\"Northern Florida\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3551,\"Y\":2609,\"Width\":282,\"Height\":282},\"MapBounds\":{\"X\":4686,\"Y\":2024,\"Width\":282,\"Height\":282},\"CenterX\":3683.9863,\"CenterY\":2728.4219,\"TerritoryType\":2,\"ID\":36,\"RegionName\":\"Eastern South Carolina\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":0,\"Y\":3380,\"Width\":350,\"Height\":327},\"MapBounds\":{\"X\":4628,\"Y\":1143,\"Width\":350,\"Height\":327},\"CenterX\":125.3006,\"CenterY\":3504.3638,\"TerritoryType\":1,\"ID\":37,\"RegionName\":\"Western Pennsylvania\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3090,\"Y\":1981,\"Width\":529,\"Height\":273},\"MapBounds\":{\"X\":3757,\"Y\":649,\"Width\":529,\"Height\":273},\"CenterX\":3332.1033,\"CenterY\":2104.476,\"TerritoryType\":1,\"ID\":38,\"RegionName\":\"Western Michigan\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3551,\"Y\":2377,\"Width\":343,\"Height\":232},\"MapBounds\":{\"X\":4688,\"Y\":965,\"Width\":343,\"Height\":232},\"CenterX\":3726.3262,\"CenterY\":2496.9448,\"TerritoryType\":1,\"ID\":39,\"RegionName\":\"Western New York\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3347,\"Y\":1580,\"Width\":444,\"Height\":249},\"MapBounds\":{\"X\":4404,\"Y\":1817,\"Width\":444,\"Height\":249},\"CenterX\":3571.2136,\"CenterY\":1712.859,\"TerritoryType\":2,\"ID\":40,\"RegionName\":\"Western North Carolina\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3428,\"Y\":3211,\"Width\":264,\"Height\":377},\"MapBounds\":{\"X\":4512,\"Y\":1367,\"Width\":264,\"Height\":377},\"CenterX\":3557.2632,\"CenterY\":3433.6357,\"TerritoryType\":1,\"ID\":41,\"RegionName\":\"West Virginia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3428,\"Y\":3588,\"Width\":404,\"Height\":177},\"MapBounds\":{\"X\":4575,\"Y\":1580,\"Width\":404,\"Height\":177},\"CenterX\":3659.6907,\"CenterY\":3675.4607,\"TerritoryType\":2,\"ID\":42,\"RegionName\":\"Western Virginia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3067,\"Y\":3486,\"Width\":346,\"Height\":268},\"MapBounds\":{\"X\":4409,\"Y\":2092,\"Width\":346,\"Height\":268},\"CenterX\":3229.3076,\"CenterY\":3612.7068,\"TerritoryType\":2,\"ID\":43,\"RegionName\":\"Northeastern Georgia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":607,\"Y\":3603,\"Width\":402,\"Height\":145},\"MapBounds\":{\"X\":4791,\"Y\":1759,\"Width\":402,\"Height\":145},\"CenterX\":823.39056,\"CenterY\":3681.1785,\"TerritoryType\":2,\"ID\":44,\"RegionName\":\"Northern North Carolina\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3347,\"Y\":1829,\"Width\":420,\"Height\":141},\"MapBounds\":{\"X\":4147,\"Y\":1757,\"Width\":420,\"Height\":141},\"CenterX\":3536.7703,\"CenterY\":1899.8533,\"TerritoryType\":1,\"ID\":45,\"RegionName\":\"Southern Kentucky\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2828,\"Y\":3652,\"Width\":204,\"Height\":222},\"MapBounds\":{\"X\":3739,\"Y\":2576,\"Width\":204,\"Height\":222},\"CenterX\":2916.608,\"CenterY\":3750.5632,\"TerritoryType\":2,\"ID\":46,\"RegionName\":\"Southern Louisiana\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3619,\"Y\":1981,\"Width\":272,\"Height\":184},\"MapBounds\":{\"X\":4863,\"Y\":1125,\"Width\":272,\"Height\":184},\"CenterX\":3766.817,\"CenterY\":2085.1216,\"TerritoryType\":1,\"ID\":47,\"RegionName\":\"Northern Pennsylvanian Appalachia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3672,\"Y\":490,\"Width\":171,\"Height\":251},\"MapBounds\":{\"X\":4223,\"Y\":2297,\"Width\":171,\"Height\":251},\"CenterX\":3758.7922,\"CenterY\":585.8457,\"TerritoryType\":2,\"ID\":48,\"RegionName\":\"Eastern Alabama\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1750,\"Y\":3559,\"Width\":157,\"Height\":292},\"MapBounds\":{\"X\":5148,\"Y\":708,\"Width\":157,\"Height\":292},\"CenterX\":1827.4529,\"CenterY\":3683.1138,\"TerritoryType\":1,\"ID\":49,\"RegionName\":\"Vermont\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2828,\"Y\":3402,\"Width\":235,\"Height\":250},\"MapBounds\":{\"X\":4285,\"Y\":2051,\"Width\":235,\"Height\":250},\"CenterX\":2924.361,\"CenterY\":3489.4216,\"TerritoryType\":2,\"ID\":50,\"RegionName\":\"Northern Georgia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1750,\"Y\":3243,\"Width\":164,\"Height\":316},\"MapBounds\":{\"X\":5267,\"Y\":669,\"Width\":164,\"Height\":316},\"CenterX\":1809.2565,\"CenterY\":3431.804,\"TerritoryType\":1,\"ID\":51,\"RegionName\":\"New Hampshire\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2227,\"Y\":3699,\"Width\":232,\"Height\":189},\"MapBounds\":{\"X\":3931,\"Y\":1746,\"Width\":232,\"Height\":189},\"CenterX\":2352.569,\"CenterY\":3799.7961,\"TerritoryType\":1,\"ID\":52,\"RegionName\":\"Western Kentucky\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3692,\"Y\":3211,\"Width\":186,\"Height\":189},\"MapBounds\":{\"X\":3856,\"Y\":1928,\"Width\":186,\"Height\":189},\"CenterX\":3787.9736,\"CenterY\":3304.599,\"TerritoryType\":2,\"ID\":53,\"RegionName\":\"Western Tennessee\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3090,\"Y\":2254,\"Width\":378,\"Height\":115},\"MapBounds\":{\"X\":4066,\"Y\":1986,\"Width\":378,\"Height\":115},\"CenterX\":3281.1394,\"CenterY\":2308.2651,\"TerritoryType\":2,\"ID\":54,\"RegionName\":\"Southern Tennessee\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1937,\"Y\":3699,\"Width\":290,\"Height\":160},\"MapBounds\":{\"X\":4136,\"Y\":2533,\"Width\":290,\"Height\":160},\"CenterX\":2098.6934,\"CenterY\":3759.4878,\"TerritoryType\":2,\"ID\":55,\"RegionName\":\"West Florida\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1009,\"Y\":3603,\"Width\":164,\"Height\":266},\"MapBounds\":{\"X\":3737,\"Y\":2320,\"Width\":164,\"Height\":266},\"CenterX\":1090.5645,\"CenterY\":3755.3481,\"TerritoryType\":2,\"ID\":56,\"RegionName\":\"Western Mississipi\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":0,\"Y\":3707,\"Width\":238,\"Height\":176},\"MapBounds\":{\"X\":4501,\"Y\":2006,\"Width\":238,\"Height\":176},\"CenterX\":116.99528,\"CenterY\":3778.1997,\"TerritoryType\":2,\"ID\":57,\"RegionName\":\"Western South Carolina\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3619,\"Y\":2165,\"Width\":277,\"Height\":168},\"MapBounds\":{\"X\":5216,\"Y\":934,\"Width\":277,\"Height\":168},\"CenterX\":3753.1262,\"CenterY\":2249.5688,\"TerritoryType\":1,\"ID\":58,\"RegionName\":\"Massachusetts\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2350,\"Y\":3478,\"Width\":184,\"Height\":192},\"MapBounds\":{\"X\":4713,\"Y\":1271,\"Width\":184,\"Height\":192},\"CenterX\":2444.5173,\"CenterY\":3585.78,\"TerritoryType\":1,\"ID\":59,\"RegionName\":\"South Pennsylvania Appalachia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3164,\"Y\":2816,\"Width\":355,\"Height\":168},\"MapBounds\":{\"X\":4373,\"Y\":2214,\"Width\":355,\"Height\":168},\"CenterX\":3315.7222,\"CenterY\":2894.488,\"TerritoryType\":2,\"ID\":60,\"RegionName\":\"Central Georgia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1196,\"Y\":3720,\"Width\":218,\"Height\":174},\"MapBounds\":{\"X\":4890,\"Y\":1566,\"Width\":218,\"Height\":174},\"CenterX\":1321.527,\"CenterY\":3790.8594,\"TerritoryType\":2,\"ID\":61,\"RegionName\":\"Eastern Virginia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2396,\"Y\":3060,\"Width\":135,\"Height\":212},\"MapBounds\":{\"X\":3768,\"Y\":2114,\"Width\":135,\"Height\":212},\"CenterX\":2464.2979,\"CenterY\":3171.7168,\"TerritoryType\":2,\"ID\":62,\"RegionName\":\"Northern Mississippi\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3647,\"Y\":1179,\"Width\":202,\"Height\":243},\"MapBounds\":{\"X\":4622,\"Y\":2115,\"Width\":202,\"Height\":243},\"CenterX\":3742.5115,\"CenterY\":1294.5126,\"TerritoryType\":2,\"ID\":63,\"RegionName\":\"Southern South Carolina\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":743,\"Y\":2751,\"Width\":284,\"Height\":75},\"MapBounds\":{\"X\":2659,\"Y\":1876,\"Width\":284,\"Height\":75},\"CenterX\":881.2223,\"CenterY\":2785.585,\"TerritoryType\":0,\"ID\":64,\"RegionName\":\"Public Land Strip\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3672,\"Y\":741,\"Width\":180,\"Height\":131},\"MapBounds\":{\"X\":4985,\"Y\":1282,\"Width\":180,\"Height\":131},\"CenterX\":3743.6375,\"CenterY\":780.99927,\"TerritoryType\":1,\"ID\":65,\"RegionName\":\"Pennsylvania\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3712,\"Y\":872,\"Width\":143,\"Height\":156},\"MapBounds\":{\"X\":4858,\"Y\":1267,\"Width\":143,\"Height\":156},\"CenterX\":3781.5903,\"CenterY\":955.1085,\"TerritoryType\":1,\"ID\":66,\"RegionName\":\"Central Pennsylvania Appalachia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3692,\"Y\":3400,\"Width\":159,\"Height\":155},\"MapBounds\":{\"X\":5219,\"Y\":1051,\"Width\":159,\"Height\":155},\"CenterX\":3763.6572,\"CenterY\":3461.6226,\"TerritoryType\":1,\"ID\":67,\"RegionName\":\"Connecticut\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3647,\"Y\":1422,\"Width\":176,\"Height\":157},\"MapBounds\":{\"X\":5065,\"Y\":1080,\"Width\":176,\"Height\":157},\"CenterX\":3746.3887,\"CenterY\":1483.3215,\"TerritoryType\":1,\"ID\":68,\"RegionName\":\"New York City Metropolitan Area\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":607,\"Y\":3748,\"Width\":119,\"Height\":142},\"MapBounds\":{\"X\":5108,\"Y\":1185,\"Width\":119,\"Height\":142},\"CenterX\":662.5218,\"CenterY\":3818.4963,\"TerritoryType\":1,\"ID\":69,\"RegionName\":\"Northern New Jersey\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2982,\"Y\":1827,\"Width\":260,\"Height\":103},\"MapBounds\":{\"X\":4725,\"Y\":1515,\"Width\":260,\"Height\":103},\"CenterX\":3073.43,\"CenterY\":1874.9764,\"TerritoryType\":2,\"ID\":70,\"RegionName\":\"Fredericksburg\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1057,\"Y\":3368,\"Width\":126,\"Height\":145},\"MapBounds\":{\"X\":5104,\"Y\":1324,\"Width\":126,\"Height\":145},\"CenterX\":1120.8096,\"CenterY\":3425.1995,\"TerritoryType\":1,\"ID\":71,\"RegionName\":\"Southern New Jersey\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3468,\"Y\":2254,\"Width\":149,\"Height\":115},\"MapBounds\":{\"X\":4939,\"Y\":1393,\"Width\":149,\"Height\":115},\"CenterX\":3529.9426,\"CenterY\":2310.4563,\"TerritoryType\":1,\"ID\":72,\"RegionName\":\"Western Chesapeake Bay\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3551,\"Y\":2891,\"Width\":194,\"Height\":97},\"MapBounds\":{\"X\":4776,\"Y\":1468,\"Width\":194,\"Height\":97},\"CenterX\":3646.9492,\"CenterY\":2931.89,\"TerritoryType\":2,\"ID\":73,\"RegionName\":\"The Wilderness\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2396,\"Y\":3272,\"Width\":129,\"Height\":190},\"MapBounds\":{\"X\":5053,\"Y\":1412,\"Width\":129,\"Height\":190},\"CenterX\":2446.2485,\"CenterY\":3380.5354,\"TerritoryType\":1,\"ID\":74,\"RegionName\":\"Eastern Chesapeake Bay\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1090,\"Y\":2217,\"Width\":98,\"Height\":161},\"MapBounds\":{\"X\":5084,\"Y\":1373,\"Width\":98,\"Height\":161},\"CenterX\":1130.9493,\"CenterY\":2313.2195,\"TerritoryType\":1,\"ID\":75,\"RegionName\":\"Delaware\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1090,\"Y\":2378,\"Width\":86,\"Height\":112},\"MapBounds\":{\"X\":3903,\"Y\":2676,\"Width\":86,\"Height\":112},\"CenterX\":1127.3774,\"CenterY\":2424.4736,\"TerritoryType\":2,\"ID\":76,\"RegionName\":\"New Orleans\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":3712,\"Y\":1028,\"Width\":173,\"Height\":106},\"MapBounds\":{\"X\":5210,\"Y\":1157,\"Width\":173,\"Height\":106},\"CenterX\":3778.0674,\"CenterY\":1083.9894,\"TerritoryType\":1,\"ID\":77,\"RegionName\":\"Long Island\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1090,\"Y\":2490,\"Width\":98,\"Height\":96},\"MapBounds\":{\"X\":4981,\"Y\":1500,\"Width\":98,\"Height\":96},\"CenterX\":1132.2906,\"CenterY\":2533.9631,\"TerritoryType\":1,\"ID\":78,\"RegionName\":\"South Chesapeake Bay\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1057,\"Y\":3513,\"Width\":129,\"Height\":57},\"MapBounds\":{\"X\":4821,\"Y\":1436,\"Width\":129,\"Height\":57},\"CenterX\":1123.6959,\"CenterY\":3540.5198,\"TerritoryType\":2,\"ID\":79,\"RegionName\":\"North Virginia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2661,\"Y\":1033,\"Width\":64,\"Height\":73},\"MapBounds\":{\"X\":4722,\"Y\":2339,\"Width\":64,\"Height\":73},\"CenterX\":2689.1682,\"CenterY\":1064.0117,\"TerritoryType\":2,\"ID\":80,\"RegionName\":\"Savannah\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2661,\"Y\":957,\"Width\":111,\"Height\":76},\"MapBounds\":{\"X\":4764,\"Y\":1435,\"Width\":111,\"Height\":76},\"CenterX\":2696.1597,\"CenterY\":984.4164,\"TerritoryType\":1,\"ID\":81,\"RegionName\":\"West Maryland\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1131,\"Y\":2586,\"Width\":50,\"Height\":88},\"MapBounds\":{\"X\":5361,\"Y\":1043,\"Width\":50,\"Height\":88},\"CenterX\":1152.6508,\"CenterY\":2624.072,\"TerritoryType\":1,\"ID\":82,\"RegionName\":\"Rhode Island\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1090,\"Y\":2768,\"Width\":66,\"Height\":45},\"MapBounds\":{\"X\":4003,\"Y\":2059,\"Width\":66,\"Height\":45},\"CenterX\":1119.8331,\"CenterY\":2788.3567,\"TerritoryType\":2,\"ID\":83,\"RegionName\":\"Shiloh\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1090,\"Y\":2693,\"Width\":51,\"Height\":75},\"MapBounds\":{\"X\":4948,\"Y\":1495,\"Width\":51,\"Height\":75},\"CenterX\":1110.3522,\"CenterY\":2721.3943,\"TerritoryType\":2,\"ID\":84,\"RegionName\":\"Manasses\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1090,\"Y\":2586,\"Width\":41,\"Height\":107},\"MapBounds\":{\"X\":5121,\"Y\":1581,\"Width\":41,\"Height\":107},\"CenterX\":1104.1978,\"CenterY\":2629.1702,\"TerritoryType\":1,\"ID\":85,\"RegionName\":\"Accomack\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2661,\"Y\":1106,\"Width\":63,\"Height\":53},\"MapBounds\":{\"X\":4886,\"Y\":1423,\"Width\":63,\"Height\":53},\"CenterX\":2695.61,\"CenterY\":1119.5795,\"TerritoryType\":1,\"ID\":86,\"RegionName\":\"Antietam\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1027,\"Y\":2751,\"Width\":40,\"Height\":43},\"MapBounds\":{\"X\":4240,\"Y\":2399,\"Width\":40,\"Height\":43},\"CenterX\":1041.6779,\"CenterY\":2768.3787,\"TerritoryType\":2,\"ID\":87,\"RegionName\":\"Montgomery\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2725,\"Y\":1033,\"Width\":40,\"Height\":46},\"MapBounds\":{\"X\":4971,\"Y\":1672,\"Width\":40,\"Height\":46},\"CenterX\":2740.2417,\"CenterY\":1053.3124,\"TerritoryType\":2,\"ID\":88,\"RegionName\":\"Richmond\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2724,\"Y\":1106,\"Width\":44,\"Height\":42},\"MapBounds\":{\"X\":4390,\"Y\":2189,\"Width\":44,\"Height\":42},\"CenterX\":2741.135,\"CenterY\":1124.0516,\"TerritoryType\":2,\"ID\":89,\"RegionName\":\"Atlanta\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1141,\"Y\":2693,\"Width\":47,\"Height\":47},\"MapBounds\":{\"X\":3778,\"Y\":2410,\"Width\":47,\"Height\":47},\"CenterX\":1165.3636,\"CenterY\":2712.3123,\"TerritoryType\":2,\"ID\":90,\"RegionName\":\"Vicksburg\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2490,\"Y\":1953,\"Width\":52,\"Height\":26},\"MapBounds\":{\"X\":4883,\"Y\":1407,\"Width\":52,\"Height\":26},\"CenterX\":2512.003,\"CenterY\":1962.5515,\"TerritoryType\":1,\"ID\":91,\"RegionName\":\"South Gettysburg\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2574,\"Y\":1953,\"Width\":28,\"Height\":27},\"MapBounds\":{\"X\":4982,\"Y\":1497,\"Width\":28,\"Height\":27},\"CenterX\":2583.3884,\"CenterY\":1962.5979,\"TerritoryType\":1,\"ID\":92,\"RegionName\":\"Washington D.C\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":2542,\"Y\":1953,\"Width\":32,\"Height\":26},\"MapBounds\":{\"X\":5103,\"Y\":1351,\"Width\":32,\"Height\":26},\"CenterX\":2551.8877,\"CenterY\":1960.3334,\"TerritoryType\":1,\"ID\":93,\"RegionName\":\"Philidelphia\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1501,\"Y\":1156,\"Width\":35,\"Height\":21},\"MapBounds\":{\"X\":4198,\"Y\":2281,\"Width\":35,\"Height\":21},\"CenterX\":1514.6962,\"CenterY\":1163.1266,\"TerritoryType\":2,\"ID\":94,\"RegionName\":\"Birmingham\",\"BitField\":\"\"},{\"TextureSource\":{\"X\":1536,\"Y\":1156,\"Width\":22,\"Height\":17},\"MapBounds\":{\"X\":4890,\"Y\":1400,\"Width\":22,\"Height\":17},\"CenterX\":1542.8468,\"CenterY\":1160.3153,\"TerritoryType\":1,\"ID\":95,\"RegionName\":\"Gettysburg\",\"BitField\":\"\"}]";
        public const string Ques = "[{\"TerritoryName\":\"Manasses\",\"Question\":\"The first Battle of Bull Run (first Battle of Manassas), was the first major land battle of the civil war. Who won this battle?\",\"Answers\":[\"Confederacy\",\"   Union   \",\"Inconclusive\"]},{\"TerritoryName\":\"Shiloh\",\"Question\":\"The battle of Shiloh was when Union forces met Confederate resistance when trying to capture a major southern railway line. What was the famous Union general at this battle?\",\"Answers\":[\"Ulysses S. Grant\",\" Winfield Scott \",\"George McClellan\"]},{\"TerritoryName\":\"Antietam\",\"Question\":\"This was a major battle where Robert E. Lee tried to take the war to the North, in order to force peace. Who won this battle?\",\"Answers\":[\"Was a draw\",\"   Union   \",\"Confederacy\"]},{\"TerritoryName\":\"Antietam\",\"Question\":\"Which Union General(who later ran for president) fought at this decisive battle?\",\"Answers\":[\"George B. McClellan\",\" Ulysses S. Grant  \",\"Rutherford B. Hayes\"]},{\"TerritoryName\":\"Gettysburg\",\"Question\":\"What speech did Lincoln deliver ar Gettysburg?\",\"Answers\":[\"Gettysburg Address\",\" Farewell Address \",\"   War on Terror   \"]},{\"TerritoryName\":\"Gettysburg\",\"Question\":\"What was the last ditch Confederate attempt to break through at Gettysburg called?\",\"Answers\":[\"Pickett's Charge\",\"Jackson's Charge\",\" There was none. \"]},{\"TerritoryName\":\"Antietam\",\"Question\":\"What was NOT related to one of the motivations for making the emancipation proclamation?\",\"Answers\":[\"  Slave rebellions  \",\"European intervention\",\"     Scope of war    \"]},{\"TerritoryName\":\"Southern Louisiana\",\"Question\":\"What was name of the Union's main strategy?\",\"Answers\":[\" Anaconda Plan \",\"  Python Plan  \",\"Armada Strategy\"]},{\"TerritoryName\":\"South Chesapeake Bay\",\"Question\":\"Which one of these was NOT an inital advantage of the North?\",\"Answers\":[\"Moral Cause\",\"Higher GDP\",\"Better Navy\"]},{\"TerritoryName\":\"West Maryland\",\"Question\":\"Which one of these was NOT an inital advantage of the South?\",\"Answers\":[\"Better trade\",\"Defensive War\",\"Good generals\"]},{\"TerritoryName\":\"Western Tennessee\",\"Question\":\"Which Confederate General instigated the Fort Pillow Massacre\",\"Answers\":[\"  Nathan Forrest \",\"Stonewall Jackson\",\"It did not exist\"]},{\"TerritoryName\":\"Southern Virginia\",\"Question\":\"Which Confederate General was in charge to put down the Bragg Rebellion\",\"Answers\":[\"It did not exist\",\"Stonewall Jackson\",\"   Sidney Drake  \"]},{\"TerritoryName\":\"Eastern Virginia\",\"Question\":\"During the Petersbug campaign, Union soldiers tunneled to Confederate lines and used explosives. What was Union's goal behind \\\"The Crater\\\"?\",\"Answers\":[\"  Break stalemate \",\"  Destroy morale  \",\"Break Supply lines\"]},{\"TerritoryName\":\"Washington D.C\",\"Question\":\"Where was the St. Albans Raid?\",\"Answers\":[\"Vermont \",\"Maryland\",\"  Maine \"]},{\"TerritoryName\":\"West Virginia\",\"Question\":\"Which of these states was NOT a border state?\",\"Answers\":[\"  Oklahoma   \",\"  Missouri   \",\"West Virginia\"]},{\"TerritoryName\":\"Fredericksburg\",\"Question\":\"What was the inital stated Union purpose of the Civil War?\",\"Answers\":[\"Preserve Union\",\"Remove Slavery\",\"State's rights\"]},{\"TerritoryName\":\"The Wilderness\",\"Question\":\"The Battle of the Wilderness marks the start of Grant's offensive towards Richmond. Who won?\",\"Answers\":[\"Inconclusive\",\"   Union    \",\"Confederacy \"]},{\"TerritoryName\":\"Eastern South Carolina\",\"Question\":\"The confederate attack on Fort Sumter is regarded as the start of the Civil War. What was the result of the engagement?\",\"Answers\":[\"South won\",\"North won\",\" Neither \"]},{\"TerritoryName\":\"Western Virginia\",\"Question\":\"The surrender at Appomattox Court House marked the end of most southern resistance. Who was the Condeferate General that surrendered?\",\"Answers\":[\"  Robert E. Lee  \",\"Stonewall Jackson\",\"  George Pickett \"]},{\"TerritoryName\":\"Fredericksburg\",\"Question\":\"Condeferate General Stonewall Jackson got his nickname at the first Battle of Bull Run. How did he die?\",\"Answers\":[\"Friendly fire\",\"  Explosion  \",\" Union Charge \"]},{\"TerritoryName\":\"Arkansas\",\"Question\":\"The Anaconda plan was the main Union strategy formulated by Winfield Scott. Which one of these was one of the goals of the Anaconda Plan?\",\"Answers\":[\"Mississpi River\",\"    Florida    \",\"     Texas     \"]},{\"TerritoryName\":\"South Georgia\",\"Question\":\"Sherman's March to the Sea was based on the idea that armies are supported by the people. It used total war tactics to disrupt the south. What was the final desination of this march?\",\"Answers\":[\"Savannah\",\"Atlanta \",\" Miami  \"]},{\"TerritoryName\":\"Savannah\",\"Question\":\"Georgia took the full force of Sherman's March to the Sea. Who was the general who instigated it?\",\"Answers\":[\"William T Sherman \",\"Franklin Roosevelt\",\"   Barack Obama   \"]},{\"TerritoryName\":\"Central Georgia\",\"Question\":\"General William T. Sherman accepted the surrender of:\",\"Answers\":[\"Joseph E. Johnston\",\"  Robert E. Lee  \",\"Stonewall Jackson\"]},{\"TerritoryName\":\"South Chesapeake Bay\",\"Question\":\"What was the primary reason the south claims as the reason for seceding?\",\"Answers\":[\"  Slavery   \",\"States Rights\",\"Trade Tariffs\"]}]";
    }
}
