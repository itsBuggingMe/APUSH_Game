using APUSH_Game.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game
{
    internal class Troop : IUpdateComponent, IPosition
    {
        public object Tag { get; set; }

        public Vector2 Position { get; set; }

        public Troop(Vector2 position)
        {
            Position = position;
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
