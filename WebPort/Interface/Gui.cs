using System;
using System.Collections;
using System.Collections.Generic;

namespace APUSH_Game.Interface
{
    /// <summary>
    /// Base gui class for the library
    /// </summary>
    public abstract class Gui : IEnumerable<Gui>
    {
        /// <summary>
        /// Global Location/Screen Location. Use for screen drawing
        /// </summary>
        public Vector2 GlobalLocation
        {
            get
            {
                if (_parent == null)
                    return LocalLocationUnscale * ScaleVector;

                return LocalLocationUnscale * ScaleVector + _parent.GlobalLocation;
            }
        }

        /// <summary>
        /// Location of the Gui Element relative to parent. Scale is in the Preferred scale set
        /// </summary>
        public Vector2 LocalLocationUnscale { get; set; }

        /// <summary>
        /// When null, is base component. Otherwise represents parent component
        /// </summary>
        public Gui Parent => _parent;
        private Gui _parent;

        /// <summary>
        /// Child Elements, drawn and updated when parent is
        /// </summary>
        protected readonly List<Gui> ChildElements = new();

        /// <summary>
        /// optional tag to differenitate different gui elements
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>
        /// Depth drawing. Use SpriteSortMode.FontToBack
        /// </summary>
        public float Depth { get; protected set; }

        public Gui(Vector2 LocalLocation)
        {
            LocalLocationUnscale = LocalLocation;
        }

        public Gui(Vector2 LocalLocation, string Tag)
        {
            LocalLocationUnscale = LocalLocation;
            this.Tag = Tag;
        }

        #region HelperFunctions

        /// <summary>
        /// Adds a child element to the Gui
        /// </summary>
        /// <returns>The object that was added</returns>
        public T AddElement<T>(T child) where T : Gui
        {
            ChildElements.Add(child);
            child.SetParent(this);
            return child;
        }

        /// <summary>
        /// Removes an child object from the Gui
        /// </summary>
        /// <param name="child">Object to be removed</param>
        /// <returns>The object that was removed</returns>
        public Gui RemoveElement(Gui child)
        {
            ChildElements.Remove(child);
            return child;
        }

        /// <summary>
        /// Iterates through all child object backwards. Use for deletion and more
        /// </summary>
        /// <param name="OnChild">Action for a child</param>
        public void ForEachChildInverse(Action<Gui> OnChild)
        {
            for (int i = ChildElements.Count - 1; i >= 0; i--)
            {
                OnChild(ChildElements[i]);
            }
        }

        /// <summary>
        /// Preforms an action on each child
        /// </summary>
        /// <param name="OnChild"></param>
        public void ForEachChild(Action<Gui> OnChild)
        {
            for (int i = 0; i < ChildElements.Count; i++)
            {
                OnChild(ChildElements[i]);
            }
        }
        #endregion HelperFunctions

        /// <summary>
        /// Updates all child Guis
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Tick(GameTime gameTime)
        {
            for (int i = 0; i < ChildElements.Count; i++)
            {
                ChildElements[i].Tick(gameTime);
            }
        }

        /// <summary>
        /// Draws all child Guis
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime)
        {
            for (int i = 0; i < ChildElements.Count; i++)
            {
                ChildElements[i].Draw(gameTime);
            }
        }

        internal void SetParent(Gui parent)
        {
            _parent = parent;
            Depth = parent == null ? 0 : parent.Depth + 1;
        }

        public IEnumerator<Gui> GetEnumerator()
        {
            return ChildElements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ChildElements.GetEnumerator();
        }

        #region StaticInit
        private static Game _game;

        /// <summary>
        /// Screen Size of the client window
        /// </summary>
        public static Vector2 ScreenSizeVec => (_game ?? throw new InvalidOperationException("Call Gui.Initalize")).Window.ClientBounds.Size.ToVector2();
        public static Point ScreenSize => (_game ?? throw new InvalidOperationException("Call Gui.Initalize")).Window.ClientBounds.Size;

        /// <summary>
        /// Scale of screen compared to default size. Larger screens will have larger vectors
        /// </summary>
        public static Vector2 ScaleVector => ScreenSizeVec / PreferredSizeVector;

        /// <summary>
        /// Default sized initalized in Gui.Initalize();
        /// </summary>
        public static Point PreferredSize { get; private set; }
        public static Vector2 PreferredSizeVector { get; private set; }

        /// <summary>
        /// Call this function in Game.Initalize()
        /// </summary>
        /// <param name="game">The game object; "this"</param>
        /// <param name="DefaultSize">The default size for the window. Local Locations will be based on this. eg. location of 50,50 and DefaultSize of 100,100 will always have that location at screen center</param>
        public static void Initalize(Game game, Point DefaultSize)
        {
            _game = game;

            PreferredSize = DefaultSize;
            PreferredSizeVector = DefaultSize.ToVector2();

            GuiHelper.GeneratePixelTexture(game.GraphicsDevice);
        }
        #endregion StaticInit
    }

}
