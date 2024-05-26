using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Interface.Elements
{
    internal class TextElement : Gui
    {
        private string _text;
        public string Text { get => _text;
            set
            {
                _text = value;
                UpdateSize();
            }
        }

        public Color Color { get; set; }
        public float Size { get; set; }

        private Vector2 _cachedSize;
        private Vector2 _alignment;

        public TextElement(Vector2 LocalLocation, string text, Color color, float size, Vector2 alignment) 
            : base(LocalLocation)
        {
            _alignment = alignment;
            Size = size;
            Color = color;
            Text = text;
            UpdateSize();
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 offset = _cachedSize * Size * _alignment;
            Globals.SpriteBatch.DrawString(
                Globals.Font, 
                _text,
                GlobalLocation - offset, 
                Color, 
                0, 
                Vector2.Zero, 
                Size, 
                SpriteEffects.None, 
                Globals.ForegroundText);
            
            base.Draw(gameTime);
        }

        private void UpdateSize()
        {
            _cachedSize = Globals.Font.MeasureString(Text);
        }
    }
}
