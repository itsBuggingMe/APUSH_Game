using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Helpers
{
    public static class Actions
    {
        public static void Empty() { }
        public static void Empty<T>(T value) { }
        public static void Empty<T1, T2>(T1 value1, T2 value2) { }
        public static void Empty<T1, T2, T3>(T1 value1, T2 value2, T3 value3) { }
    }
}
