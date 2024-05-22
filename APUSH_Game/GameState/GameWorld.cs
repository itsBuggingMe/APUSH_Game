using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class GameWorld
    {
        private EntityManager _em;
        private County[] _counties;

        public GameWorld()
        {
            _counties = JsonConvert.DeserializeObject<County[]>(File.ReadAllText("us-county-clean.json"));
            Array.Sort(_counties, (a, b) => a.LocationX.CompareTo(b.LocationX));
            _em = new(this);

            _em.AddEntity(new CountyUpdate(_counties), new CountyDraw());
        }

        public void Tick(GameTime Gametime)
        {
            _em.Update(Gametime);
        }

        public void Draw(GameTime Gametime)
        {
            _em.Draw(Gametime);
        }
    }
}
