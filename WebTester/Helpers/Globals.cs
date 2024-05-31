using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Helpers
{
    /// <summary>
    /// Contains global variables and settings for the game.
    /// </summary>
    internal static class Globals
    {
        /// <summary>
        /// Reference to the current camera
        /// </summary>
        //public static GameCamera Camera;

        /// <summary>
        /// The graphics device for rendering.
        /// </summary>
        public static GraphicsDevice Graphics;

        /// <summary>
        /// The graphics device manager for managing graphics settings.
        /// </summary>
        public static GraphicsDeviceManager GraphicsManager;

        /// <summary>
        /// The sprite batch for 2D rendering.
        /// </summary>
        public static SpriteBatch SpriteBatch;


        public static SpriteFont Font;

        /// <summary>
        /// Gets the size of the game window.
        /// </summary>
        public static Point WindowSize => new(GameRoot.Game.Window.ClientBounds.Width, GameRoot.Game.Window.ClientBounds.Height);

        public static Action<Point> WindowSizeEvent = null;

        public static Action OnFocusLost = null;

        public static Action OnFocusGained = null;

        public static int TotalFrames { get; private set; }

        /// <summary>
        /// The default size of the game window.
        /// </summary>
        public static Vector2 DefaultSize => new(1920, 1080);

        public static float Scale { get; private set; }
        public static Vector2 ScaleVec { get; private set; }

        public static Texture2D Pixel { get; private set; }

        /// <summary>
        /// Initializes global variables and settings.
        /// </summary>
        /// <param name="game">The main game instance.</param>
        /// <param name="graphicsDeviceManager">The graphics device manager.</param>
        public static void Initialize(Game game, GraphicsDeviceManager graphicsDeviceManager)
        {
            Graphics = game.GraphicsDevice;
            GraphicsManager = graphicsDeviceManager;

            SpriteBatch = new SpriteBatch(Graphics);

            Scale = WindowSize.V().Length() / DefaultSize.Length();
            ScaleVec = WindowSize.V() / DefaultSize;
            WindowSizeEvent += (p) => Scale = p.V().Length() / DefaultSize.Length();
            WindowSizeEvent += (p) => ScaleVec = p.V() / DefaultSize;

            Pixel = new Texture2D(game.GraphicsDevice, 1, 1);
            Pixel.SetData(new Color[] { Color.White });
        }

        public static void Update(GameTime gameTime)
        {
            TotalFrames++;
        }

        public static void LoadFont(string path)
        {
            Font = GameRoot.Game.Content.Load<SpriteFont>(path);
        }
    }
}
