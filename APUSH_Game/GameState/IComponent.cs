using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal interface IComponent
    {
        public object Tag { get; set; }
        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId) { }
        public void Update(GameTime gameTime) { }
    }

    internal interface IDrawComponent : IComponent
    {

    }

    internal interface IUpdateComponent : IComponent
    {

    }
}
