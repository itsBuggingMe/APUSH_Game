using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Helpers
{
    internal interface IScreen
    {
        public void Tick(GameTime Gametime);

        public void Draw(GameTime Gametime);

        public void OnStateEnter(IScreen previous, object transferInfo);
        public object OnStateExit();
    }
}
