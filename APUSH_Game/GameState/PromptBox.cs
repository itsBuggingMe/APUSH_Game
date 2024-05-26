using APUSH_Game.Helpers;
using static APUSH_Game.GameState.PromptBox;

namespace APUSH_Game.GameState
{
    internal class PromptBox : IGameObject
    {
        public const int GridSize = 64;
        public object Tag { get; set; }

        private string _prompt;
        public Rectangle PromptBounds { get; private set; }
        private Rectangle _promptBoundStatic;
        public bool IsMouseOver { get; private set; }

        public string Text => _prompt;
        public Color Color => Color.White;
        public float Size => 0.3f * Globals.Scale * sizem;
        public Vector2 Position => _windowPortion * Globals.WindowSize.V() + offsetVector;
        private Vector2 _windowPortion;
        public bool Post => true;
        public bool Reactive { get; init; }
        private Action _onClick;
        private float sizem;
        public PromptBox(string text, Vector2 windowPortion, bool reactive = true, Action onClick = null, float sizem = 1)
        {
            this.sizem = sizem;
            _onClick = onClick;
            Reactive = reactive;
            _prompt = text;
            _windowPortion = windowPortion;
            CalcuatePromptBounds();

            _bg = GameRoot.Game.Content.Load<Texture2D>("DialogeBorder");

            //memory leak but i dont have time to fix
            Globals.WindowSizeEvent += p => CalcuatePromptBounds();
        }

        private void CalcuatePromptBounds()
        {
            Vector2 size = Globals.Font.MeasureString(_prompt) * 1.1f * Size;
            Point sizeP = (size + new Vector2(64)).ToPoint();
            sizeP.X = Math.Max(128, sizeP.X);
            sizeP.Y = Math.Max(128, sizeP.Y);
            sizeP = new Point((int)MathF.Ceiling(sizeP.X / GridSize) * GridSize,
                (int)MathF.Ceiling(sizeP.Y / GridSize) * GridSize);
            _promptBoundStatic = Helper.RectangleFromCenterSize(Position.ToPoint(), sizeP);
            PromptBounds = _promptBoundStatic.OffsetCopy(offset);
        }

        private Vector2 offsetVector;
        private float offsetVelocity;
        private Point offset;

        public void Update()
        {
            IsMouseOver = _promptBoundStatic.Contains(InputHelper.MouseLocation);

            offsetVelocity = 0;
            if(Reactive && IsMouseOver)
            {
                offsetVelocity += 5;
                if(InputHelper.Down(MouseButton.Left))
                {
                    offsetVelocity += 5;
                    _onClick?.Invoke();
                }
            }
            offsetVector += new Vector2(offsetVelocity);
            offsetVector *= 0.7f;
            offset = offsetVector.ToPoint();

            PromptBounds = _promptBoundStatic.OffsetCopy(offset);
        }

        public void Draw()
        {
            if (Globals.TotalFrames % 15 == 0)
            {
                int y = (Corner.Y + 128) % (3 * 128);
                Corner.Y = y;
                HSide.Y = y;
                VSide.Y = y;
            }

            GameRoot.Game.PostDraw += DoDraw;
        }

        private void DoDraw(GameTime gt)
        {
            Rectangle bounds = _promptBox.PromptBounds;

            Rectangle topLeft = new Rectangle(bounds.X, bounds.Y, GridSize, GridSize);
            int topCount = bounds.Width / GridSize;
            int leftCount = bounds.Height / GridSize;

            Globals.SpriteBatch.Draw(Globals.Pixel, new Rectangle(bounds.X + GridSize / 2, bounds.Y + GridSize / 2, bounds.Width - GridSize, bounds.Height - GridSize), Color.Black);

            Rectangle copy = topLeft;
            Draw(Corner, ref copy, GridSize);
            for (int i = 2; i < topCount; i++)
                Draw(HSide, ref copy, GridSize);
            Draw(Corner, ref copy, 0, GridSize, se: SpriteEffects.FlipHorizontally);
            for (int i = 2; i < leftCount; i++)
                Draw(VSide, ref copy, 0, GridSize, se: SpriteEffects.FlipHorizontally);

            copy = topLeft.OffsetCopy(new Point(0, GridSize));
            for (int i = 2; i < leftCount; i++)
                Draw(VSide, ref copy, 0, GridSize);
            Draw(Corner, ref copy, GridSize, se: SpriteEffects.FlipVertically);
            for (int i = 2; i < topCount; i++)
                Draw(HSide, ref copy, GridSize, se: SpriteEffects.FlipVertically);

            Draw(Corner, ref copy, GridSize, se: SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally);
        }

        private void Draw(Rectangle source, ref Rectangle screen, int offsetX = 0, int offsetY = 0, SpriteEffects se = SpriteEffects.None)
        {
            Globals.SpriteBatch.Draw(_bg, screen, source, Color.White, default, default, se, 0);
            screen.Offset(offsetX, offsetY);
        }

        private PromptBox _promptBox;
        private Texture2D _bg;
        private Rectangle Corner = new Rectangle(0, 0, GridSize, GridSize);
        private Rectangle HSide = new Rectangle(128, 0, GridSize, GridSize);
        private Rectangle VSide = new Rectangle(2 * 128, 0, GridSize, GridSize);
    }
}
