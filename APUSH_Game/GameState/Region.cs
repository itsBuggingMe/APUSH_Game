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
            _size = Math.Clamp(Data.MapBounds.Size.ToVector2().Length() * 0.0005f, 0.08f, 0.5f);
            Position = new Vector2(Data.CenterX, Data.CenterY) - Data.TextureSource.Location.V() + Data.MapBounds.Location.V();
            ID = iD;
        }

        public Troop CurrentTroops { get; set; }

        public bool IsSelected { get; private set; }
        public bool MouseOver { get; private set; }
        public bool LightUp { get; set; }
        public Vector2 Position { get; private set; }
        public Region Data { get; init; }
        private GameWorld world;
        private float _size;
        public readonly int ID;

        public void AddTroop(int amt)
        {

        }

        public void RemoveTroops(int amt)
        {

        }

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
            if (!world.State.AllowSelect(this))
            {
                IsSelected = MouseOver = false;
                return;
            }

            Point worldMousePos = world.Camera.ScreenToWorld(InputHelper.MouseLocation.ToVector2()).ToPoint();
            MouseOver = IsInTerritory(worldMousePos);

            if (InputHelper.FallingEdge(MouseButton.Left))
            {
                if(IsSelected)
                {
                    IsSelected = false;
                    world.State.RegionDeselected(this);
                }
                else if(MouseOver)
                {
                    IsSelected = true;
                    world.State.RegionSelected(this);
                }
            }
        }

        public static readonly Color UnionTerritory = new Color(112, 146, 190);
        public static readonly Color UnionState = new Color(91, 125, 217);
        public static readonly Color ConfederateState = new Color(158, 158, 158);
        public static readonly ImmutableArray<Color> Colors = new Color[] { UnionTerritory, UnionState, ConfederateState }.ToImmutableArray();
        private Texture2D _texture;

        public void Draw()
        {
            if(LightUp)
            {
                DrawLight();
            }
            else
                if (MouseOver)
                {
                    if (InputHelper.Down(MouseButton.Left))
                    {
                        DrawLight();
                    }
                    else
                    {
                        DrawOffset();
                    }
                }
                else if(IsSelected)
                {
                    DrawOffset();
                }
                else
                {
                    DrawNormal();
                }

            //draw text
            Color textColor = (MouseOver || IsSelected) ? Color.White : (Color.White * (_size > 0.2f ? 1 - TransparencyFunction(world.Camera.Zoom) : TransparencyFunction(world.Camera.Zoom)));
            Globals.SpriteBatch.DrawStringCentered(Data.RegionName, 
                Data.MapBounds.Location.V() +  
                new Vector2(Data.CenterX, Data.CenterY) - 
                Data.TextureSource.Location.V(), _size, textColor, Depth.TextWorld);


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

        public void ReportDeselect()
        {
            IsSelected = false;
        }

        public override string ToString()
        {
            return Data.RegionName;
        }

        private float TransparencyFunction(float x) => 1f / (1f + (float)Math.Exp(-(x * 20 - 10 * _size - 10)));
    }
}
