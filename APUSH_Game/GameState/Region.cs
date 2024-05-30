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
        public RegionObject(Region r, GameWorld world, int iD, int troopCount)
        {
            Data = r;
            CurrentTroops = new(TroopType.Infantry, troopCount, this, Data.TerritoryType != TerrioryType.ConfederateState);
            _texture = GameRoot.Game.Content.Load<Texture2D>("Packed");
            this.world = world;
            const float largestRec = 0.000523923539f;
            _size = Math.Clamp(Data.MapBounds.Size.ToVector2().Length() * largestRec, 0, 0.95f);
            Position = new Vector2(Data.CenterX, Data.CenterY) - Data.TextureSource.Location.V() + Data.MapBounds.Location.V();
            ID = iD;
            _transparency = GetTransparency();
            _numTransparency = 0;
        }

        public Troop CurrentTroops { get; set; }

        public bool IsSelected => world.State.Primary == this || world.State.Secondary == this;
        public bool MouseOver { get; private set; }
        public bool LightUp { get; set; }
        public Vector2 Position { get; private set; }
        public Region Data { get; init; }
        private GameWorld world;
        private float _size;
        private Smoother _transparency;
        private Smoother _numTransparency;
        public readonly int ID;

        public TerrioryType Type
        {
            get => Data.TerritoryType;
            set => Data.TerritoryType = value;
        }
        public bool IsInTerritory(Point p)
        {
            return Data.MapBounds.Contains(p) && world.GetColorAtLoc(p - Data.MapBounds.Location + Data.TextureSource.Location).A > 0;
        }

        public void Update()
        {
            LightUp = world.State.IsPotenialSelection(this);
            Point worldMousePos = world.Camera.ScreenToWorld(InputHelper.MouseLocation.ToVector2()).ToPoint();
            MouseOver = IsInTerritory(worldMousePos);

            if (InputHelper.FallingEdge(MouseButton.Left) && Helper.TaxicabDistance(InputHelper.MouseLocation, InputHelper.LeftMouseDownLoc) < 4)
            {
                if(IsSelected)
                    world.State.TryRegionDeselected(this);
                else if (MouseOver && LightUp)
                    world.State.TryRegionSelected(this);
            }

            _transparency.Approach((MouseOver || IsSelected) ? 1 : GetTransparency());
            _numTransparency.Approach(smoothingConstant: 0.3f);
        }

        public static readonly Color UnionTerritory = new Color(112, 146, 190);
        public static readonly Color UnionState = new Color(91, 125, 217);
        public static readonly Color ConfederateState = new Color(158, 40, 32);
        public static readonly ImmutableArray<Color> Colors = new Color[] { UnionTerritory, UnionState, ConfederateState }.ToImmutableArray();
        private Texture2D _texture;

        private float GetTransparency()
        {
            return NewTransparency(_size, world.Camera.Zoom) > 0.5f ? 1 : 0;
            static float NewTransparency(float size, float zoom)
            {
                size -= 0.8f;
                zoom -= 0.5f;
                return SmoothInOutZero(size * 0.85f + zoom * 0.2f, 1.5f);
            }
        }

        public void Draw()
        {
            if (IsSelected)
                DrawOffset();
            else if (LightUp)
                DrawLight();
            else
                DrawNormal();

            //draw text


            Vector2 loc = Data.MapBounds.Location.V() +
                new Vector2(Data.CenterX, Data.CenterY) -
                Data.TextureSource.Location.V();

            if (_transparency > 0.1f)
                _numTransparency.Target = 0;
            else
                _numTransparency.Target = 1;

            Globals.SpriteBatch.DrawStringCentered($"{Data.RegionName}: {CurrentTroops.TroopCount}", loc,
                0.12f / world.Camera.Zoom * _transparency, Color.White * _transparency, Depth.TextWorld);
            Globals.SpriteBatch.DrawStringCentered($"{CurrentTroops.TroopCount}", loc, 
                0.06f * _size * 30 * _numTransparency, Color.White * _numTransparency, Depth.TextWorld);

            void DrawNormal()
            {
                Globals.SpriteBatch.Draw(
                    _texture,
                    Data.MapBounds,
                    Data.TextureSource,
                    Colors[(int)Data.TerritoryType] * 0.7f,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    Depth.Territory);
            }

            void DrawLight()
            {
                Globals.SpriteBatch.Draw(
                    _texture,
                    Data.MapBounds,
                    Data.TextureSource,
                    Colors[(int)Data.TerritoryType] * 0.85f,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    Depth.Territory);
            }

            void DrawOffset()
            {
                Globals.SpriteBatch.Draw(
                    _texture,
                    Data.MapBounds,
                    Data.TextureSource,
                    Colors[(int)Data.TerritoryType] * 0.7f,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    Depth.Territory);

                
                Globals.SpriteBatch.Draw(
                    _texture, 
                    Data.MapBounds.OffsetCopy(new Point(-3)), 
                    Data.TextureSource, 
                    Colors[(int)Data.TerritoryType], 
                    0, 
                    Vector2.Zero, 
                    SpriteEffects.None,
                    Depth.Territory + Depth.Epsilon);
            }
        }

        public override string ToString()
        {
            return Data.RegionName;
        }

        private static float SmoothInOutZero(float x, float pSize)
        {
            x *= 4;
            float range0_1 =  x > 0 ? MathF.Tanh(-x + pSize) + 1 : MathF.Tanh(x + pSize) + 1;
            return MathF.Pow(range0_1, 12);
        }
    }
}
