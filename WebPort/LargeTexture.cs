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

        private readonly Color[] TLcolors;
        private readonly Color[] TRcolors;
        private readonly Color[] BLcolors;
        private readonly Color[] BRcolors;

        public LargeTexture(string name, bool generateColors = false)
        {
            Name = name;
            tl = GameRoot.Game.Content.Load<Texture2D>($"{name}tl");
            tr = GameRoot.Game.Content.Load<Texture2D>($"{name}tr");
            bl = GameRoot.Game.Content.Load<Texture2D>($"{name}bl");
            br = GameRoot.Game.Content.Load<Texture2D>($"{name}br");

            Width = tl.Width + tr.Width;
            Height = tl.Height + bl.Height;

            Bounds = new Rectangle(0, 0, Width, Height);

            TLBounds = tl.Bounds.OffsetCopy(new Point(0, 0));
            TRBounds = tr.Bounds.OffsetCopy(new Point(tl.Width, 0));
            BRBounds = br.Bounds.OffsetCopy(new Point(tl.Width, tl.Height));
            BLBounds = bl.Bounds.OffsetCopy(new Point(0, tl.Height));

            if (generateColors)
            {
                TLcolors = new Color[tl.Width * tl.Height];
                TRcolors = new Color[tr.Width * tr.Height];
                BLcolors = new Color[bl.Width * bl.Height];
                BRcolors = new Color[br.Width * br.Height];

                tl.GetData(TLcolors);
                tr.GetData(TRcolors);
                bl.GetData(BLcolors);
                br.GetData(BRcolors);
            }
        }

        public Color GetColorAtPosition(Point p)
        {
            if (p.X < 0 || p.Y < 0 || p.X >= Width || p.Y >= Height)
                return Color.Transparent;

            if (TLBounds.Contains(p))
            {
                int index = p.Y * TLBounds.Width + p.X;
                return TLcolors[index];
            }
            else if (TRBounds.Contains(p))
            {
                int localX = p.X - TRBounds.X;
                int index = p.Y * TRBounds.Width + localX;
                return TRcolors[index];
            }
            else if (BLBounds.Contains(p))
            {
                int localY = p.Y - BLBounds.Y;
                int index = localY * BLBounds.Width + p.X;
                return BLcolors[index];
            }
            else if (BRBounds.Contains(p))
            {
                int localX = p.X - BRBounds.X;
                int localY = p.Y - BRBounds.Y;
                int index = localY * BRBounds.Width + localX;
                return BRcolors[index];
            }

            return Color.Transparent;
        }
    }

    internal static class SpriteBatchTextureExtensions
    {
        public static void DrawLarge(this SpriteBatch sb, LargeTexture texture, Color color)
        {// oh so hardcoded
            sb.Draw(texture.tl, Vector2.Zero, null, color, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
            sb.Draw(texture.tr, new Vector2(4096, 0), null, color, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
        }

        private static int tlCount;
        private static readonly DrawCall[] stackTL = new DrawCall[50];
        private static int trCount;
        private static readonly DrawCall[] stackTR = new DrawCall[50];
        private static int blCount;
        private static readonly DrawCall[] stackBL = new DrawCall[50];
        private static int brCount;
        private static readonly DrawCall[] stackBR = new DrawCall[50];

        public static void DrawLarge(this SpriteBatch sb, LargeTexture texture, Rectangle dest, Rectangle source, Color color)
        {
            Rectangle sourceIntersectTL = Intersect(source, defaultTL);
            if (sourceIntersectTL != default)
                stackTL[tlCount++] = new DrawCall(texture.tl, dest.Location.ToVector2(), sourceIntersectTL, color);

            Rectangle sourceIntersectTR = Intersect(source, defaultTR);
            if (sourceIntersectTR != default)
                stackTR[trCount++] = new DrawCall(texture.tr, new Vector2(sourceIntersectTL.Width + dest.X, dest.Y), sourceIntersectTR.OffsetCopy(new Point(-2048, 0)), color);

            Rectangle sourceIntersectBL = Intersect(source, defaultBL);
            if (sourceIntersectBL != default)
                stackBL[blCount++] = new DrawCall(texture.bl, new Vector2(dest.X, dest.Y + sourceIntersectTL.Height), sourceIntersectBL.OffsetCopy(new Point(0, -2048)), color);

            Rectangle sourceIntersectBR = Intersect(source, defaultBR);
            if (sourceIntersectBR != default)
                stackBR[brCount++] = new DrawCall(texture.br, new Vector2(dest.X + Math.Max(sourceIntersectTL.Width, sourceIntersectBL.Width), dest.Y + Math.Max(sourceIntersectTL.Height, sourceIntersectTR.Height)), sourceIntersectBR.OffsetCopy(new Point(-2048)), color);
        }


        public static void FlushTerritoryCalls()
        {
            for (int i = 0; i < tlCount; i++)
                stackTL[i].DrawDrawCall();
            tlCount = 0;

            for (int i = 0; i < trCount; i++)
                stackTR[i].DrawDrawCall();
            trCount = 0;

            for (int i = 0; i < blCount; i++)
                stackBL[i].DrawDrawCall();
            blCount = 0;

            for (int i = 0; i < brCount; i++)
                stackBR[i].DrawDrawCall();
            brCount = 0;
        }


        internal static void DrawDrawCall(ref this DrawCall data)
        {
            Globals.SpriteBatch.Draw(data.texture, data.destination, data.source, data.color);
        }

        internal readonly record struct DrawCall(Texture2D texture, Vector2 destination, Rectangle source, Color color);

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
