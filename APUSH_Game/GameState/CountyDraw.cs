using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class CountyDraw : IDrawComponent
    {
        private CountyUpdate counties;

        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId)
        {
            counties = manager.GetOrThrow<CountyUpdate>(thisEntityId);
        }

        public void Update(GameTime gameTime)
        {
            Vector2 mp = InputHelper.MouseLocation.ToVector2() + Vector2.One * 500 + Vector2.UnitX * 1000;
            var locarr = counties.Counties;
            for (int i = 0; i < locarr.Length; i++)
            {
                Vector2 cOffset = mp + new Vector2(locarr[i].LocationX, -locarr[i].LocationY);
                float[] xLocs = locarr[i].OutlineX;
                float[] yLocs = locarr[i].OutlineY;
                Vector2 prev = new Vector2(xLocs[0], -yLocs[0]) * 25 + cOffset;
                int stride = Math.Max(xLocs.Length / 70, 1);
                for(int j = 1; j < xLocs.Length; j += stride)
                {
                    Vector2 @new = new Vector2(xLocs[j], -yLocs[j]) * 25 + cOffset;
                    Globals.ShapeBatch.DrawLine(prev, @new, 2, Color.Black, Color.Gray);
                    prev = @new;
                }
            }
        }
    }

    internal class CountyUpdate : IUpdateComponent
    {
        public static bool IsDraw => false;
        public bool InstIsDraw => false;
        public County[] Counties { get; private set; }

        public CountyUpdate(County[] counties)
        {
            Counties = counties;
        }

        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId)
        {

        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
