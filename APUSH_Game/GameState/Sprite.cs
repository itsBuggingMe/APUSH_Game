using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class Sprite : IDrawComponent
    {
        public object Tag { get; set; }

        private IPosition DrawLoc;
        private IColored Color;
        private Texture2D texture;
        private Rectangle source;
        private Vector2 align;
        private float layerDepth;
        private float scale;
        public Sprite(string name, Vector2 alignment = default, Rectangle source = default, float layerDepth = 0, float scale = 1)
            : this(GameRoot.Game.Content.Load<Texture2D>(name), alignment, source, layerDepth, scale) { }

        public Sprite(Texture2D text, Vector2 alignment = default, Rectangle source = default, float layerDepth = 0, float scale = 1)
        {
            this.scale = scale;
            this.layerDepth = layerDepth;
            this.source = source;
            this.align = alignment;
            texture = text;
        }

        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId)
        {
            DrawLoc = manager.GetOrThrow<IPosition>(thisEntityId);
            if (manager.TryGetComponent(thisEntityId, out IColored color))
                Color = color;
            else
                Color = DefaultColor.Instance;
        }

        public void Update(GameTime gameTime)
        {
            Globals.SpriteBatch.Draw(texture, DrawLoc.Position, source, Color.Color, 0, align, scale, SpriteEffects.None, layerDepth);
        }
    }

    internal class PrimitiveRect : IDrawComponent
    {
        public object Tag { get; set; }
        private IBounds bounds;
        private IColored color;

        public PrimitiveRect(float layerDepth)
        {

        }

        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId)
        {
            manager
                .OutGetOrThrow(thisEntityId, out bounds)
                .OutGetOrThrow(thisEntityId, out color);
        }

        public void Update(GameTime gameTime)
        {

        }
    }

    internal interface IPosition : IUpdateComponent
    {
        public Vector2 Position { get; }
    }

    internal interface IColored : IUpdateComponent
    {
        public Color Color { get; }
    }

    internal interface IBounds : IUpdateComponent
    {
        public Rectangle Bounds { get; }
    }

    internal class DefaultColor : IColored
    {
        public static readonly DefaultColor Instance = new();
        public Color Color => Color.White;
        public object Tag { get; set; }
    }
}
