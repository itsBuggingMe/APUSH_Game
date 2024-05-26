using APUSH_Game.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class RegionObject : IGameObject
    {
        public bool Delete => false;
        public RegionObject(Region r, GameWorld world)
        {
            _texture = GameRoot.Game.Content.Load<Texture2D>("Packed");
            Data = r;
            this.world = world;
            _size = Math.Clamp(Data.MapBounds.Size.ToVector2().Length() * 0.0005f, 0.08f, 0.5f);
            Position = new Vector2(Data.CenterX, Data.CenterY) - Data.TextureSource.Location.V() + Data.MapBounds.Location.V();
        }

        public bool IsSelected { get; private set; }
        public Vector2 Position { get; private set; }
        public Region Data { get; init; }
        private GameWorld world;
        private float _size;

        public void Update()
        {
            Point worldMousePos = world.Camera.ScreenToWorld(InputHelper.MouseLocation.ToVector2()).ToPoint();
            IsSelected = Data.MapBounds.Contains(worldMousePos) && world.GetColorAtLoc(worldMousePos - Data.MapBounds.Location + Data.TextureSource.Location).A > 0;

            if (IsSelected && InputHelper.RisingEdge(MouseButton.Left) && false)
            {

            }
        }

        public static readonly Color UnionTerritory = new Color(112, 146, 190);
        public static readonly Color UnionState = new Color(91, 125, 217);
        public static readonly Color ConfederateState = new Color(158, 158, 158);
        public static readonly ImmutableArray<Color> Colors = new Color[] { UnionTerritory, UnionState, ConfederateState }.ToImmutableArray();
        private Texture2D _texture;

        public void Draw()
        {
            if (IsSelected)
            {
                Globals.SpriteBatch.Draw(
                    _texture,
                    Data.MapBounds,
                    Data.TextureSource,
                    Colors[(int)Data.TerritoryType] * 0.7f,
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
                    Data.MapBounds,
                    Data.TextureSource,
                    Colors[(int)Data.TerritoryType] * 0.9f,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    Globals.TerritoryLayer);
            }

            //draw text
            Color textColor = Color.White * (_size > 0.2f ? 1 - TransparencyFunction(world.Camera.Zoom) : TransparencyFunction(world.Camera.Zoom));
            Globals.SpriteBatch.DrawStringCentered(Data.RegionName, 
                Data.MapBounds.Location.V() +  
                new Vector2(Data.CenterX, Data.CenterY) - 
                Data.TextureSource.Location.V(), _size, textColor);
        }

        private float TransparencyFunction(float x) => 1f / (1f + (float)Math.Exp(-(x * 20 - 10 * _size - 10)));
    }
}
