using FastBitmapLib;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    internal static class MapProcessor
    {
        private static readonly Color[] pallate = [Color.FromArgb(255, 255, 255), Color.FromArgb(0,0,0), Color.FromArgb(209, 219, 221)];
        private static readonly int[] pallateARGB = [Color.FromArgb(255, 255, 255).ToArgb(), Color.FromArgb(0,0,0).ToArgb(), Color.FromArgb(209, 219, 221).ToArgb()];
        
        public static void P1(FastBitmap bmp)
        {
            bmp.ModPixel(c => pallate.MinBy(pc => Math.Abs(pc.R - c.R) + Math.Abs(pc.G - c.G) + Math.Abs(pc.B - c.B)));
        }

        public static void P2(FastBitmap bmp)
        {
            bmp.ModPixel((x, y, c) =>
            {
                return c == pallate[2]
                    &&
                    (
                        bmp.GetPixel(x + 1, y).ToArgb() == Color.White.ToArgb() ||
                        bmp.GetPixel(x - 1, y).ToArgb() == Color.White.ToArgb() ||
                        bmp.GetPixel(x, y - 1).ToArgb() == Color.White.ToArgb() ||
                        bmp.GetPixel(x, y + 1).ToArgb() == Color.White.ToArgb()
                    ) ? Color.Black : c;
            });
        }

        static HashSet<Point>[] wSort;
        static HashSet<Point>[] bSort;
        static HashSet<Point>[] tSort;

        public static void P3(FastBitmap bmp)
        {
            Stack<HashSet<Point>> whiteGroups = new();
            Stack<HashSet<Point>> blackGroups = new();
            Stack<HashSet<Point>> tealGroups = new();

            bmp.ModPixel((x, y, c) =>
            {
                if(c == pallate[0] && !whiteGroups.Any(h => h.Contains(new Point(x, y))))
                    whiteGroups.Push(FloodFill(bmp, x, y));
                if (c == pallate[1] && !blackGroups.Any(h => h.Contains(new Point(x, y))))
                    blackGroups.Push(FloodFill(bmp, x, y));
                if (c == pallate[2] && !tealGroups.Any(h => h.Contains(new Point(x, y))))
                    tealGroups.Push(FloodFill(bmp, x, y));
                return c;
            });

            wSort = whiteGroups.OrderByDescending(s => s.Count).ToArray();
            bSort = blackGroups.OrderByDescending(s => s.Count).ToArray();
            tSort = tealGroups.OrderByDescending(s => s.Count).ToArray();

            foreach(var point in tSort.Skip(96).SelectMany(h => h))
            {
                bmp.SetPixel(point.X, point.Y, Color.Magenta);
            }
            foreach (var point in bSort.Skip(1).SelectMany(h => h))
            {
                bmp.SetPixel(point.X, point.Y, Color.Magenta);
            }
            foreach (var point in wSort.Skip(1).SelectMany(h => h))
            {
                bmp.SetPixel(point.X, point.Y, Color.Magenta);
            }
       }

        public static void P4(FastBitmap bmp)
        {
            bmp.ModPixel(c => c.EqualsValue(Color.Magenta) ? Color.White : c);
        }


        public static void P5(FastBitmap bmp)
        {
            Stack<HashSet<Point>> whiteGroups = new();

            bmp.ModPixel((x, y, c) =>
            {
                if (c == pallate[0] && !whiteGroups.Any(h => h.Contains(new Point(x, y))))
                    whiteGroups.Push(FloodFill(bmp, x, y));
                return c;
            });

            wSort = whiteGroups.OrderByDescending(s => s.Count).ToArray();

            foreach (var point in wSort.Skip(1).SelectMany(h => h))
            {
                bmp.SetPixel(point.X, point.Y, Color.Black);
            }
        }

        public static void P6(FastBitmap bmp)
        {
            bmp.ModPixel((x, y, c) => 
            c.EqualsValue(Color.White) &&
            (
                (bmp.Validate(x + 1, y) && bmp.GetPixel(x + 1, y).EqualsValue(pallate[2])) ||
                (bmp.Validate(x - 1, y) && bmp.GetPixel(x - 1, y).EqualsValue(pallate[2])) ||
                (bmp.Validate(x, y - 1) && bmp.GetPixel(x, y - 1).EqualsValue(pallate[2])) ||
                (bmp.Validate(x, y + 1) && bmp.GetPixel(x, y + 1).EqualsValue(pallate[2]))
            ) ? Color.Black : c);
        }

        public static void P7(FastBitmap bmp)
        {
            Stack<HashSet<Point>> sections = new();

            bmp.ModPixel((x, y, c) =>
            {
                if (c == pallate[2] && !sections.Any(h => h.Contains(new Point(x, y))))
                    sections.Push(FloodFill(bmp, x, y));
                return c;
            });

            tSort = sections.OrderByDescending(s => s.Count).ToArray();

            List<Point> toAdd = new();

            for(int i = 0; i < 70; i++)
            {
                foreach (var state in tSort)
                {
                    toAdd.Clear();

                    foreach (var p in state)
                    {//every border point
                        if (bmp.TryGetColor(p.Add(new(1, 0)), out Color _c) && _c.EqualsValue(Color.Black))
                            ExplorePoint(bmp, toAdd, state, p.Add(new(1, 0)));
                        else if (bmp.TryGetColor(p.Add(new(-1, 0)), out _c) && _c.EqualsValue(Color.Black))
                            ExplorePoint(bmp, toAdd, state, p.Add(new(-1, 0)));
                        else if (bmp.TryGetColor(p.Add(new(0, 1)), out _c) && _c.EqualsValue(Color.Black))
                            ExplorePoint(bmp, toAdd, state, p.Add(new(0, 1)));
                        else if (bmp.TryGetColor(p.Add(new(0, -1)), out _c) && _c.EqualsValue(Color.Black))
                            ExplorePoint(bmp, toAdd, state, p.Add(new(0, -1)));
                    }

                    foreach (var item in toAdd)
                        state.Add(item);
                }
            }
        }

        public static void P8(FastBitmap bmp)
        {
            P6(bmp);
        }

        public static void ExplorePoint(FastBitmap bmp, List<Point> toAdd, HashSet<Point> statePts, Point p)
        {
            int x = p.X;
            int y = p.Y;
            if(!(
                (bmp.Validate(x + 1, y) && (bmp.GetPixel(x + 1, y).EqualsValue(pallate[2]) && !statePts.Contains(new Point(x + 1, y)))) ||
                (bmp.Validate(x - 1, y) && (bmp.GetPixel(x - 1, y).EqualsValue(pallate[2]) && !statePts.Contains(new Point(x - 1, y)))) ||
                (bmp.Validate(x, y - 1) && (bmp.GetPixel(x, y - 1).EqualsValue(pallate[2]) && !statePts.Contains(new Point(x, y - 1)))) ||
                (bmp.Validate(x, y + 1) && (bmp.GetPixel(x, y + 1).EqualsValue(pallate[2]) && !statePts.Contains(new Point(x, y + 1))))
                ))
            {
                toAdd.Add(p);
                bmp.SetPixel(x, y, pallate[2]);
            }
        }

        public static HashSet<Point> FloodFill(FastBitmap bmp, int x, int y)
        {
            HashSet<Point> filledPoints = new HashSet<Point>();
            Color targetColor = bmp.GetPixel(x, y);

            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(x, y));

            while (stack.Count > 0)
            {
                Point current = stack.Pop();

                if (!filledPoints.Contains(current) &&
                    current.X >= 0 && current.X < bmp.Width &&
                    current.Y >= 0 && current.Y < bmp.Height &&
                    bmp.GetPixel(current.X, current.Y) == targetColor)
                {
                    filledPoints.Add(current);

                    stack.Push(new Point(current.X + 1, current.Y));
                    stack.Push(new Point(current.X - 1, current.Y));
                    stack.Push(new Point(current.X, current.Y + 1));
                    stack.Push(new Point(current.X, current.Y - 1));
                }
            }

            return filledPoints;
        }
        public static void ModPixel(this FastBitmap bmp, Func<Color, Color> selector)
        {
            for(int i = 0; i < bmp.Width; i++)
            {
                for(int j = 0; j < bmp.Height; j++)
                {
                    bmp.SetPixel(i, j, selector(bmp.GetPixel(i, j)));
                }
            }
        }
        public static void ModPixel(this FastBitmap bmp, Func<int, int, Color, Color> selector)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    bmp.SetPixel(i, j, selector(i, j, bmp.GetPixel(i, j)));
                }
            }
        }
        public static bool Validate(this FastBitmap bmp, int x, int y)
        {
            return (uint)x < bmp.Width && (uint)y < bmp.Height;
        }
        public static bool EqualsValue(this Color bmp, Color other)
        {
            return bmp.ToArgb() == other.ToArgb();
        }

        public static bool TryGetColor(this FastBitmap bmp, Point p, out Color color)
        {
            if(bmp.Validate(p.X, p.Y))
            {
                color = bmp.GetPixel(p.X, p.Y);
                return true;
            }
            color = default;
            return false;
        }

        public static Point Add(this Point p, Point other)
        {
            return new Point(p.X + other.X, p.Y + other.Y);
        }
    }
}
