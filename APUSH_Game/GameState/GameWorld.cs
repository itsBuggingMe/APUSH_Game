using System.IO;
using System.Collections.Generic;
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
        private readonly Region[] _regions;
        private readonly GameCamera gameCamera;
        public GameCamera Camera => gameCamera;

        private readonly Texture2D _bg;
        private readonly Texture2D _pk;
        private readonly Color[] colors;

        private readonly List<IGameObject> GameObjects = new List<IGameObject>();

        public GameWorld()
        {
            gameCamera = new GameCamera();
            gameCamera.Location = new Vector2(3150.7664f,  1723.9828f);
            gameCamera.Zoom = 0.284573227f;
            _bg = GameRoot.Game.Content.Load<Texture2D>("mapBg");

            _pk = GameRoot.Game.Content.Load<Texture2D>("Packed");
            colors = new Color[_pk.Width * _pk.Height];
            _pk.GetData(colors);

            _regions = JsonConvert.DeserializeObject<Region[]>(File.ReadAllText("terr.json"));
            for(int i = 0; i < _regions.Length; i++)
            {
                GameObjects.Add(new RegionObject(_regions[i], this));
            }
        }

        private Vector2 cameraVelocity = Vector2.Zero;

        private float zoomMValue = 1;

        const float accelerationRate = 0.3f;
        const float decelerationRate = 0.98f;



        public void Tick(GameTime Gametime)
        {   
            for(int i = GameObjects.Count - 1; i >= 0; i--)
            {
                GameObjects[i].Update();
            }

            Vector2 accel = Vector2.Zero;
            if (InputHelper.Down(Keys.W))
                accel += new Vector2(0, -accelerationRate);
            if (InputHelper.Down(Keys.S))
                accel += new Vector2(0, accelerationRate);
            if (InputHelper.Down(Keys.A))
                accel += new Vector2(-accelerationRate, 0);
            if (InputHelper.Down(Keys.D))
                accel += new Vector2(accelerationRate, 0);
            if (InputHelper.DeltaScroll != 0)
                zoomMValue = InputHelper.DeltaScroll > 0 ? 1.05f : 0.95f;

            gameCamera.Zoom *= zoomMValue;
            zoomMValue += (1 - zoomMValue) * 0.1f;

            cameraVelocity += accel;
            cameraVelocity *= decelerationRate;

            if (InputHelper.Down(MouseButton.Left))
                gameCamera.Location += (InputHelper.PrevMouseState.Position.V() - InputHelper.MouseLocation.V()) / gameCamera.Zoom;

            gameCamera.Location += cameraVelocity / gameCamera.Zoom;
        }

        private static readonly Color BGColor = new Color(0, 162, 232);
        public void Draw(GameTime Gametime)
        {
            Globals.Graphics.Clear(BGColor);
            
            gameCamera.StartSpriteBatch(Globals.SpriteBatch);
            Globals.SpriteBatch.Draw(_bg, Vector2.Zero, Color.White);
            for (int i = GameObjects.Count - 1; i >= 0; i--)
            {
                GameObjects[i].Draw();
            }
            Globals.SpriteBatch.End();
            
            Globals.SpriteBatch.Begin();
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

        public void AskQuestion(string question, int correctIndex, params string[] responses)
        {
            throw new NotImplementedException();
            /*
            int mainbox = _em.AddEntity(new PromptBox(question, new Vector2(0.5f, 0.38f), false, null, 0.8f), new PromptBoxDrawer(), new TextDrawer());
            PromptBox p = _em.GetOrThrow<PromptBox>(mainbox);

            Rectangle smallRect = new Rectangle(p.PromptBounds.X, p.PromptBounds.Bottom - 48, p.PromptBounds.Width, 256);

            int[] ids = new int[responses.Length + 1];
            ids[ids.Length - 1] = mainbox;

            for (int i = 0; i < responses.Length; i++)
            {
                ids[i] = _em.AddEntity(new PromptBox(responses[i], smallRect.Center.V() / Globals.WindowSize.V(), true, Clear, 0.5f), new PromptBoxDrawer(), new TextDrawer());
                smallRect.Offset(0, 128 + 16);
            }

            void Clear()
            {
                for(int i = 0; i < ids.Length; i++)
                {
                    _em.RemoveEntity(ids[i]);
                }
            }*/
        }
    }
}
