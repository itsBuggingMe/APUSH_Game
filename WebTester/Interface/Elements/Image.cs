using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Interface.Elements
{
    internal class Image : Gui
    {
        private Texture2D _texture;
        private Color _tint;
        private Vector2 _scale;
        private Vector2 _aligment;
        private Rectangle? _source;
        public Image(Vector2 location, Color tint, Vector2 scale, Vector2 alignment, string textureName, Rectangle? source = null) : base(location)
        {
            _texture = GameRoot.Game.Content.Load<Texture2D>(textureName);
            _tint = tint;
            _scale = scale;
            _aligment = alignment;
            _source = source;
        }

        public override void Draw(GameTime gameTime)
        {
            Rectangle dest = new Rectangle(GlobalLocation.P(), (ScaleVector * _scale * (_source ?? _texture.Bounds).Size.V()).P());
            dest.Offset(-(_aligment * dest.Size.V()));
            Globals.SpriteBatch.Draw(_texture, dest, _source, _tint);

            base.Draw(gameTime);
        }
    }
}
