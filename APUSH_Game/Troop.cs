using APUSH_Game.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game
{
    internal class Troop
    {
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
