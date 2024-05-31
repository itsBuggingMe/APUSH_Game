using Microsoft.Xna.Framework;

namespace APUSH_Game.Interface
{
    public abstract class BoundedGui : Gui
    {
        public Vector2 GlobalSize => originalSize * ScaleVector;
        public Rectangle Bounds => new Rectangle(GlobalLocation.ToPoint(), GlobalSize.ToPoint());

        public BoundedGui(Rectangle Bounds) : base(Bounds.Location.ToVector2())
        {
            originalSize = Bounds.Size.ToVector2();
        }

        public BoundedGui(Point Location, Point Size) : this(new Rectangle(Location, Size)) { }

        private readonly Vector2 originalSize;
    }
}
