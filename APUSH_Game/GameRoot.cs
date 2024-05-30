using System.IO;
using System;
using APUSH_Game.Helpers;
using APUSH_Game.GameState;
using APUSH_Game.Interface;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tools")]
namespace APUSH_Game
{
    public class GameRoot : Game
    {
        private readonly GraphicsDeviceManager _graphics;

        public Action<GameTime> PostDraw = null;

        public static GameRoot Game { get; private set; }

        public GameRoot()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

            if (Game != null)
                throw new InvalidOperationException("WHAT THE FUCK");
            Game = this;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Globals.Initialize(this, _graphics);
            Globals.LoadFont("Font");
            Gui.Initalize(this, new Point(1920, 1080));
            ToggleBorderless();

            ScreenManager.Initalise(new MainGameState());
        }

        protected override void Update(GameTime gameTime)
        {
            InputHelper.TickUpdate();

            Globals.Update(gameTime);

            if (InputHelper.Down(Keys.Escape))
                Exit();
            if (InputHelper.RisingEdge(Keys.F11))
                ToggleBorderless();

            ScreenManager.Instance.Tick(gameTime);
            AnimationPool.Instance.Update(gameTime);

            InputHelper.LateUpdate();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ScreenManager.Instance.Draw(gameTime);
            base.Draw(gameTime);
        }

        private void ToggleBorderless()
        {
            if (Window.IsBorderless)
            {
                Window.IsBorderless = false;

                Window.Position = (new Point(_graphics.GraphicsDevice.DisplayMode.Width, _graphics.GraphicsDevice.DisplayMode.Height).ToVector2() * 0.5f - new Vector2(400, 240)).ToPoint();

                _graphics.PreferredBackBufferWidth = 800;
                _graphics.PreferredBackBufferHeight = 480;
            }
            else
            {
                Window.IsBorderless = true;
                _graphics.PreferredBackBufferWidth = _graphics.GraphicsDevice.DisplayMode.Width;
                _graphics.PreferredBackBufferHeight = _graphics.GraphicsDevice.DisplayMode.Height;
                Window.Position = Point.Zero;
            }

            _graphics.ApplyChanges();

            Window_ClientSizeChanged(this, null);
        }

        public void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();

            Globals.WindowSizeEvent.Invoke(new Point(Window.ClientBounds.Width, Window.ClientBounds.Height));
        }
    }
}
