using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal interface IText : IUpdateComponent
    {
        string Text { get; }
        Color Color { get; }
        float Size { get; }
        Vector2 Position { get; }
        bool Post => false;
    }

    internal class TextDrawer : IDrawComponent
    {
        public object Tag { get; set; }

        private IText _text;
        public void Initalize(GameWorld world, EntityManager manager, int thisEntityId)
        {
            _text = manager.GetOrThrow<IText>(thisEntityId);
        }

        public void Update(GameTime gameTime)
        { 
            if(_text.Post)
                GameRoot.Game.PostDraw += g => Globals.SpriteBatch.DrawStringCentered(_text.Text, _text.Position, _text.Size, _text.Color);
            else
                Globals.SpriteBatch.DrawStringCentered(_text.Text, _text.Position, _text.Size, _text.Color);
        }
    }
}
