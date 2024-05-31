using APUSH_Game.GameState;
using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game
{
    internal class CursorMessage : IGameObject
    {
        private float _decayTime = 60;
        private string _msg;
        public CursorMessage(string msg) => _msg = msg;
        public bool Delete { get; private set; }
        public void Update() { }

        public void Draw()
        {
            _decayTime--;
            GameRoot.Game.PostDraw += DoDraw;
        }

        private void DoDraw(GameTime g)
        {
            float transparency = _decayTime < 20 ? _decayTime / 20f : 1;

            Rectangle bounds = new Rectangle(InputHelper.MouseLocation, (Globals.Font.MeasureString(_msg) * 0.1f).ToPoint() + new Point(32, 16));
            bounds.Offset(0, -bounds.Height);

            Globals.SpriteBatch.Draw(Globals.Pixel, bounds, Color.Black * transparency);
            bounds.Inflate(-2,-2);
            Globals.SpriteBatch.Draw(Globals.Pixel, bounds, Color.White * transparency);
            bounds.Inflate(-2, -2);
            Globals.SpriteBatch.Draw(Globals.Pixel, bounds, Color.Black * transparency);
            Globals.SpriteBatch.DrawStringCentered(_msg, bounds.Center.ToVector2(), 0.1f, Color.White * transparency);

            if(_decayTime < 0)
                Delete = true;
        }
    }
}
