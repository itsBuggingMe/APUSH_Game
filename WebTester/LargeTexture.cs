using APUSH_Game;
using APUSH_Game.Helpers;
using Microsoft.Xna.Framework.Graphics;

namespace WebPort
{
    public class LargeTexture
    {
        internal readonly Texture2D tr;
        internal readonly Texture2D tl;
        internal readonly Texture2D bl;
        internal readonly Texture2D br;
        internal readonly string Name;

        public int Width { get; }
        public int Height { get; }
        public Rectangle Bounds { get; }
        public readonly Rectangle TLBounds;
        public readonly Rectangle TRBounds;
        public readonly Rectangle BRBounds;
        public readonly Rectangle BLBounds;

        public LargeTexture(string name)
        {
            Name = name;
            tl = GameRoot.Game.Content.Load<Texture2D>($"{name}tl");
            tr = GameRoot.Game.Content.Load<Texture2D>($"{name}tr");
            bl = GameRoot.Game.Content.Load<Texture2D>($"{name}bl");
            br = GameRoot.Game.Content.Load<Texture2D>($"{name}br");

            Width = tl.Width + tr.Width;
            Height = tl.Height + bl.Height;

            Bounds = new Rectangle(0, 0, Width, Height);

            TLBounds = tl.Bounds.OffsetCopy(new Point(0,0));
            TRBounds = tr.Bounds.OffsetCopy(new Point(2048, 0));
            BRBounds = br.Bounds.OffsetCopy(new Point(2048, 2048));
            BLBounds = bl.Bounds.OffsetCopy(new Point(0, 2048));
        }

        public Color GetColorAtPosition(Point p)
        {
            if(p.X < 0 || p.Y < 0 || p.X >= Width || p.Y >= Width)
                return Color.Transparent;

            return default;
        }
    }

    internal static class SpriteBatchTextureExtensions
    {
        public static void DrawLarge(this SpriteBatch sb, LargeTexture texture, Color color)
        {// oh so hardcoded
            return;
            sb.Draw(texture.tl, Vector2.Zero, null, color, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
            sb.Draw(texture.tr, new Vector2(4096, 0), null, color, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
        }


        public static void DrawLarge(this SpriteBatch sb, LargeTexture texture, Rectangle dest, Rectangle source, Color color,
            float rot/*ignore*/, Vector2 origin, SpriteEffects spriteEffects/*ignore*/, float depth)
        {
            Rectangle sourceIntersectTL = Intersect(source, defaultTL);
            if(sourceIntersectTL != default)
                sb.Draw(texture.tl, dest.Location.V(), sourceIntersectTL, color);

            Rectangle sourceIntersectTR = Intersect(source, defaultTR);
            if (sourceIntersectTR != default)
                sb.Draw(texture.tr, new Vector2(sourceIntersectTL.Width + dest.X, dest.Y), sourceIntersectTR.OffsetCopy(new Point(-2048, 0)), color);

            Rectangle sourceIntersectBL = Intersect(source, defaultBL);
            if (sourceIntersectBL != default)
                sb.Draw(texture.bl, new Vector2(dest.X, dest.Y + sourceIntersectTL.Height), sourceIntersectBL.OffsetCopy(new Point(0, -2048)), color);

            Rectangle sourceIntersectBR = Intersect(source, defaultBR);
            if (sourceIntersectBR != default)
                sb.Draw(texture.br, new Vector2(dest.X + Math.Max(sourceIntersectTL.Width, sourceIntersectBL.Width), dest.Y + Math.Max(sourceIntersectTL.Height, sourceIntersectTR.Height)), sourceIntersectBR.OffsetCopy(new Point(-2048)), color);
        }

        private static readonly Rectangle defaultTL = new Rectangle(0,0,2048,2048);
        private static readonly Rectangle defaultTR = new Rectangle(2048,0,2048,2048);
        private static readonly Rectangle defaultBR = new Rectangle(2048,2048,2048,2048);
        private static readonly Rectangle defaultBL = new Rectangle(0,2048,2048,2048);
        public static Rectangle Intersect(Rectangle a, Rectangle b)
        {
            int x1 = Math.Max(a.X, b.X);
            int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Max(a.Y, b.Y);
            int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

            if (x2 > x1 && y2 > y1)
            {
                return new Rectangle(x1, y1, x2 - x1, y2 - y1);
            }

            return default;
        }
    }
}
