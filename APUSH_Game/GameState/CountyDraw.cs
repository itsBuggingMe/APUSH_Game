using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class RegionDrawer : IDrawComponent
    {
        public object Tag { get; set; }

        public static readonly Color UnionTerritory = new Color(112, 146, 190);
        public static readonly Color UnionState = new Color(91, 125, 217);
        public static readonly Color ConfederateState = new Color(158, 158, 158);
        public static readonly ImmutableArray<Color> Colors = new Color[] { UnionTerritory, UnionState, ConfederateState }.ToImmutableArray();

        private RegionData _regionData;
        private Texture2D _texture;
        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId)
        {
            _regionData = manager.GetOrThrow<RegionData>(thisEntityId);
            _texture = GameRoot.Game.Content.Load<Texture2D>("Packed");
        }

        public void Update(GameTime gameTime)
        {
            if (_regionData.IsSelected)
            {
                Globals.SpriteBatch.Draw(
                    _texture, 
                    _regionData.Data.MapBounds,
                    _regionData.Data.TextureSource, 
                    Colors[(int)_regionData.Data.TerritoryType] * 0.7f, 
                    0, 
                    Vector2.Zero, 
                    SpriteEffects.None, 
                    Globals.TerritoryLayer);

                /*
                Globals.SpriteBatch.Draw(
                    _texture, 
                    _regionData.Data.MapBounds.OffsetCopy(new Point(-5)), 
                    _regionData.Data.TextureSource, 
                    Colors[(int)_regionData.Data.TerritoryType], 
                    0, 
                    Vector2.Zero, 
                    SpriteEffects.None, 
                    Globals.TerritoryLayer);*/
            }
            else
            {
                Globals.SpriteBatch.Draw(
                    _texture, 
                    _regionData.Data.MapBounds, 
                    _regionData.Data.TextureSource, 
                    Colors[(int)_regionData.Data.TerritoryType] * 0.9f, 
                    0, 
                    Vector2.Zero, 
                    SpriteEffects.None, 
                    Globals.TerritoryLayer);
            }
        }
    }

    internal class RegionData : IText
    {
        public object Tag { get; set; }

        public Region Data { get; init; }
        public RegionData(Region r) => Data = r;
        public bool IsSelected { get; private set; }

        public string Text => Data.RegionName;

        public Color Color => Color.White * (Size > 0.2f ? 1 - TransparencyFunction(world.Camera.Zoom) : TransparencyFunction(world.Camera.Zoom));

        public float Size { get; private set; }

        public Vector2 Position { get; private set; }

        private GameWorld world;

        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId)
        {
            this.world = world;
            Size = Math.Clamp(Data.MapBounds.Size.ToVector2().Length() * 0.0005f, 0.08f, 0.5f);
            Position = new Vector2(Data.CenterX, Data.CenterY) - Data.TextureSource.Location.V() + Data.MapBounds.Location.V();
        }

        public void Update(GameTime gameTime)
        {
            Point worldMousePos = world.Camera.ScreenToWorld(InputHelper.MouseLocation.ToVector2()).ToPoint();
            IsSelected = Data.MapBounds.Contains(worldMousePos) && world.GetColorAtLoc(worldMousePos - Data.MapBounds.Location + Data.TextureSource.Location).A > 0;

            if(IsSelected && InputHelper.RisingEdge(MouseButton.Left) && false)
            {

            }
        }

        private float TransparencyFunction(float x)
        {
            return 1f / (1f + (float)Math.Exp(-(x * 20 - 10 * Size - 10)));
        }
    }

    internal class Question
    {
        public string Prompt { get; set; } = "";
        public string[] Answers { get; set; } = new string[] { "", "" };
        public int CorrectIndex { get; set; } = 0;
        public string QuestionTrigger { get; set; } = "";
    }

    internal class Region
    {
        public Rectangle TextureSource { get; set; }
        public Rectangle MapBounds { get; set; }
        public float CenterX { get; set; }
        public float CenterY { get; set; }
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
