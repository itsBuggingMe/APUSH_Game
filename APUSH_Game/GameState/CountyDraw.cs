using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class Question
    {
        public string Prompt { get; set; } = "";
        public string[] Answers { get; set; } = new string[] { "", "" };
        public int CorrectIndex { get; set; } = 0;
        public string QuestionTrigger { get; set; } = "";
    }

    internal class Region
    {
        public Rectangle TextureSource { get; set; }
        public Rectangle MapBounds { get; set; }
        public float CenterX { get; set; }
        public float CenterY { get; set; }
        public TerrioryType TerritoryType { get; set; }
        public int ID { get; set; }
        public string RegionName { get; set; } = string.Empty;
        public string BitField { get; set; } = string.Empty;
    }

    internal enum TerrioryType
    {
        UnionTerritory = 0,
        UnionState = 1,
        ConfederateState = 2,
    }
}
