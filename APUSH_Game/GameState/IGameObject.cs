using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal interface IGameObject
    {
        void Update();
        void Draw();
        public bool Delete { get; }
    }
}
