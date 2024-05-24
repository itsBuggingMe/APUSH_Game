using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APUSH_Game.Helpers;

namespace APUSH_Game.GameState
{
    internal class GameWorld
    {
        private EntityManager _em;
        private Region[] _regions;

        public GameWorld()
        {
            _em = new(this);
            _regions = JsonConvert.DeserializeObject<Region[]>(File.ReadAllText("terr.json"));
            for(int i = 0; i < _regions.Length; i++)
            {
                _em.AddEntity(new RegionData(_regions[i]), new RegionDrawer());
            }
        }

        public void Tick(GameTime Gametime)
        {   
            _em.Update(Gametime);
        }

        public void Draw(GameTime Gametime)
        {
            Globals.SpriteBatch.Begin();
            _em.Draw(Gametime);
            Globals.SpriteBatch.End();
        }
    }
}
