using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game
{
    public class County
    {
        public string StateName { get; set; }
        public string Name { get; set; }
        public float[] OutlineX { get; set; }
        public float[] OutlineY { get; set; }
        public float LocationX { get; set; }
        public float LocationY { get; set; }

        public County(string stateName, string name, float[] outlineX, float[] outlineY, float locationX, float locationY)
        {
            StateName = stateName;
            Name = name;
            OutlineX = outlineX;
            OutlineY = outlineY;
            LocationX = locationX;
            LocationY = locationY;
        }
    }
}
