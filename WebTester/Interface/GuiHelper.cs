namespace APUSH_Game.Interface
{
    internal static class GuiHelper
    {
        public static float ScaleTextToBounds(SpriteFont font, string text, Point size)
        {
            Vector2 textSize = font.MeasureString(text);

            float scaleX = size.X / textSize.X;
            float scaleY = size.Y / textSize.Y;

            return Math.Min(scaleX, scaleY);
        }

        public static Rectangle RectangleFromCenterSize(Point center, Point size)
        {
            return new Rectangle(center - new Point(size.X >> 1, size.Y >> 1), size);
        }

        internal static void GeneratePixelTexture(GraphicsDevice graphicsDevice)
        {
            _singlePixel = new Texture2D(graphicsDevice, 1, 1);
            _singlePixel.SetData(new Color[] { Color.White });
        }

        public static Texture2D SingleWhitePixel => _singlePixel ?? throw new InvalidOperationException("Call Gui.Initalize");
        private static Texture2D _singlePixel;
    }
}
