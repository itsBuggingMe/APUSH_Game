using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game
{
    internal static class Depth
    {
        public static readonly float Background = 0.Ex();
        public static readonly float Territory = 20.Ex();
        public static readonly float OnTerritory = 40.Ex();
        public static readonly float OverTerritory = 60.Ex();
        public static readonly float TextWorld = 80.Ex();


        public const float Epsilon = 0.00001f;
        public const float TwoEpsilon = 0.00002f;
        public const float ThreeEpsilon = 0.00003f;

        public static float Transform(int x)
        {
            return (MathF.Tanh((x - 40) * 0.01f) + 1) * 0.5f;
        }

        private static float Ex(this int x)
        {
            return Transform(x);
        }
    }
}
