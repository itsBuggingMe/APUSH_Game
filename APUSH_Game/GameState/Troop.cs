using APUSH_Game.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class Troop : IGameObject
    {
        private string dispString;
        public TroopType Type => _type;
        private TroopType _type;
        public bool Delete { get; set; }
        private Texture2D _texture;

        public static readonly ImmutableArray<Rectangle> Sources = new Rectangle[] {
            new Rectangle(0, 0, 384, 256),
            new Rectangle(384, 0, 384, 256),
            new Rectangle(768, 0, 384, 256)
        }.ToImmutableArray();

        public static readonly ImmutableArray<string> Names = new string[] {
            "Infantry",
            "Artillery",
            "Cavalry"
        }.ToImmutableArray();

        public bool Side { get; init; }
        public Color Color => Side ? Color.CornflowerBlue : Color.Gray;

        private float _hoverDistance;
        private RegionObject regionObject;
        private int _troopCount;
        public int TroopCount { get => _troopCount; 
            set
            {
                _troopCount = value;
                dispString = _troopCount.ToString();
            }
        }
        private Vector2 location;
        private Vector2 targetLocation;
        private bool isLazy = true;
        private Vector2 velocity;

        public Troop(TroopType type, int size, RegionObject parentRegion, bool side)
        {
            _texture = GameRoot.Game.Content.Load<Texture2D>("NatoIcon");

            regionObject = parentRegion;
            _hoverDistance = parentRegion.Data.MapBounds.Size.ToVector2().Length() / 8;
            _type = type;
            TroopCount = size;
            Side = side;
            location = PickPossibleWanderLocation();
            targetLocation = location;
        }

        public void Update()
        {
            if(Vector2.DistanceSquared(targetLocation, location) > 4)
            {
                velocity += Helper.UnitVectorPoint(location, targetLocation) * (isLazy ? 0.08f : 0.2f);
            }
            else
            {
                targetLocation = PickPossibleWanderLocation();
                isLazy = true;
            }

            location += velocity;
            velocity *= 0.9f;
        }

        public void Draw()
        {
            Rectangle bounds = Helper.RectangleFromCenterSize(location.P(), new Point(75, 50));
            if ((!InputHelper.Down(MouseButton.Left)) && (regionObject.MouseOver || regionObject.IsSelected))
                bounds.Offset(new Point(-3));

            Globals.SpriteBatch.Draw(Globals.Pixel, bounds, null, Color, 0, Vector2.Zero, SpriteEffects.None, Depth.OverTerritory);
            Globals.SpriteBatch.Draw(_texture, bounds, Sources[(int)_type], Color.White, 0, Vector2.Zero, SpriteEffects.None, Depth.OverTerritory + Depth.Epsilon);
            Globals.SpriteBatch.DrawStringCentered(dispString, bounds.Center.V(), 0.3f, Color.Brown, Depth.OverTerritory + Depth.TwoEpsilon);
        }

        public void MoveTo(RegionObject newRegion)
        {
            throw new NotImplementedException();
            //newRegion.CurrentTroops.Add(this);
            //regionObject.CurrentTroops.Remove(this);
            regionObject = newRegion;

            targetLocation = PickPossibleWanderLocation();
            isLazy = false;
        }

        private Vector2 PickPossibleWanderLocation()
        {
            return new Vector2(regionObject.Position.X, regionObject.Position.Y);
            var location = new Vector2(
                Helper.ApproximateNormal(regionObject.Position.X, _hoverDistance),
                Helper.ApproximateNormal(regionObject.Position.Y, _hoverDistance)
                );

            if(!regionObject.IsInTerritory(location.ToPoint()))
            {
                return PickPossibleWanderLocation();
            }

            return location;
        }
    }

    internal enum TroopType
    {
        Infantry = 0,
        Artillery = 1,
        Cavalry = 2,
    }
}
