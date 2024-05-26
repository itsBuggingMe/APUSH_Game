using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APUSH_Game.Helpers;

namespace APUSH_Game.GameState
{
    internal class MainGameState : IScreen
    {
        private GameWorld world;
        public MainGameState()
        {
            world = new GameWorld();
        }

        public void Tick(GameTime Gametime)
        {
            world.Tick(Gametime);

        }

        public void Draw(GameTime Gametime)
        {
            world.Draw(Gametime);
        }

        #region ForLater
        public void OnStateEnter(IScreen previous, object transferInfo)
        {

        }

        public object OnStateExit()
        {
            return null;
        }
        #endregion
    }
}
