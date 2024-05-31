using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game
{
    internal static class GameInfo
    {
        public static readonly string BaseDirectory = GetBaseDirectory();
        public static readonly string ContentDirectory = Path.Combine(BaseDirectory, "Assets");

        private static string GetBaseDirectory()
        {
            return Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        }
    }
}
