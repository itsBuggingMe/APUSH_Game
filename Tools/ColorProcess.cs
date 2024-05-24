using FastBitmapLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tools.MapProcessor;

namespace Tools
{
    internal class ColorProcess
    {
        public static void P1(FastBitmap bmp)
        {
            bmp.ModPixel(c => c.EqualsValue(Color.White) ? bmp.GetPixel(0, 0) : c);
        }

        private static readonly Color[] possibleColors = new Color[]
        {
            Color.FromArgb(112, 146, 190),//terr
            Color.FromArgb(91, 125, 217),//u state
            Color.FromArgb(158, 158, 158)//c
        };

        public static void P2(FastBitmap bmp)
        {
            Stack<HashSet<Point>> terriories = new();
            Stack<HashSet<Point>> unionStates = new();
            Stack<HashSet<Point>> confederacy = new();
            Stack<HashSet<Point>> SEEE = new();

            bmp.ModPixel((x, y, c) =>
            {
                if (c.EqualsValue(possibleColors[0]) && !terriories.Any(h => h.Contains(new Point(x, y))))
                    terriories.Push(FloodFill(bmp, x, y));
                if (c.EqualsValue(possibleColors[1]) && !unionStates.Any(h => h.Contains(new Point(x, y))))
                    unionStates.Push(FloodFill(bmp, x, y));
                if (c.EqualsValue(possibleColors[2]) && !confederacy.Any(h => h.Contains(new Point(x, y))))
                    confederacy.Push(FloodFill(bmp, x, y));
                if (c.EqualsValue(bmp.GetPixel(0,0)) && !SEEE.Any(h => h.Contains(new Point(x, y))))
                    SEEE.Push(FloodFill(bmp, x, y));
                return c;
            });

            var tSort = terriories.OrderByDescending(s => s.Count).ToArray();
            var uSort = unionStates.OrderByDescending(s => s.Count).ToArray();
            var cSort = confederacy.OrderByDescending(s => s.Count).ToArray();

            HashSet<Point> sharedRemoval = new();
            for(int i = 0; i < 3; i++)
            {
                ShrinkBounds(bmp, tSort.SelectMany(s => s));
                ShrinkBounds(bmp, uSort.SelectMany(s => s));
                ShrinkBounds(bmp, cSort.SelectMany(s => s));
                ShrinkBounds(bmp, SEEE.SelectMany(s => s));
            }
        }

        public static void ShrinkBounds(FastBitmap bmp, IEnumerable<Point> pts)
        {
            List<Point> toAdd = new();
            foreach(var p in pts)
            {
                int x = p.X;
                int y = p.Y;

                if (
                    (bmp.Validate(x + 1, y) && bmp.GetPixel(x + 1, y).EqualsValue(Color.Black)) ||
                    (bmp.Validate(x - 1, y) && bmp.GetPixel(x - 1, y).EqualsValue(Color.Black)) ||
                    (bmp.Validate(x, y - 1) && bmp.GetPixel(x, y - 1).EqualsValue(Color.Black)) ||
                    (bmp.Validate(x, y + 1) && bmp.GetPixel(x, y + 1).EqualsValue(Color.Black))
                    )
                {
                    toAdd.Add(p);
                }
            }

            toAdd.ForEach(p => bmp.SetPixel(p.X, p.Y, Color.Black));
        }

        public static void Pack(FastBitmap bmp)
        {

        }
    }
}
