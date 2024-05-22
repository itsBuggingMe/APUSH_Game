using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Helpers
{
    internal class ScreenManager
    {
        private static ScreenManager _instance;
        private IScreen _currentState;

        /// <summary>
        /// The singleton instance of the StateMachine
        /// </summary>
        public static ScreenManager Instance => _instance;

        private ScreenManager(IScreen initalState) => _currentState = initalState;

        /// <summary>
        /// Initalises the State Machine. Please call only once
        /// </summary>
        public static void Initalise(IScreen initalState, object info = null)
        {
            _instance = new ScreenManager(initalState);
            initalState.OnStateEnter(initalState, info);
        }

        public void ChangeState(IScreen newState)
        {
            newState.OnStateEnter(_currentState, _currentState.OnStateExit());
            _currentState = newState;
        }

        public void Tick(GameTime Gametime) => _currentState.Tick(Gametime);

        public void Draw(GameTime Gametime) => _currentState.Draw(Gametime);


        public IScreen CurrentState => _currentState;
    }

}
