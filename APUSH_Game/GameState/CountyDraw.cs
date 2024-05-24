using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class RegionDrawer : IDrawComponent
    {
        private RegionData _regionData;
        private Texture2D _texture;
        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId)
        {
            _regionData = manager.GetOrThrow<RegionData>(thisEntityId);
            _texture = GameRoot.Game.Content.Load<Texture2D>("Packed");
        }

        public void Update(GameTime gameTime)
        {
            Point pos = (InputHelper.MouseLocation - new Point(700, 400)) * new Point(8);
        
            Globals.SpriteBatch.Draw(_texture, _regionData.Data.MapBounds.OffsetCopy(pos), _regionData.Data.TextureSource, Color.White);
        }
    }

    internal class RegionData : IUpdateComponent
    {
        public Region Data { get; init; }
        public RegionData(Region r) => Data = r;

        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId)
        {

        }

        public void Update(GameTime gameTime)
        {

        }
    }

    internal class Region
    {
        public Rectangle TextureSource { get; set; }
        public Rectangle MapBounds { get; set; }
        public TerrioryType TerritoryType { get; set; }
        public int ID { get; set; }
        public string RegionName { get; set; } = string.Empty;
        public string BitField { get; set; } = string.Empty;
    }

    internal enum TerrioryType
    {
        UnionTerritory = 0,
        UnionState = 1,
        ConfederateState = 2,
    }
}
