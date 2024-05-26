using APUSH_Game.Interface.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Interface
{
    internal class GameGUI : Gui
    {
        private int _numDollars;
        public int NumDollars
        {
            get => _numDollars;
            set
            {
                _numDollars = value;
                money.Text = value.ToString();
            }
        }

        private TextElement money;

        public GameGUI() : base(Vector2.Zero)
        {
            money = AddElement(new TextElement(new Vector2(200, 100), "0", Color.Black, 0.5f, ElementAlign.LeftMiddle));
            AddElement(new Image(new Vector2(100, 100), Color.White, new Vector2(0.3f), ElementAlign.Center, "MoneySymbol"));
        }

        #region ProbrablyNotGoingToUse
        public override void Tick(GameTime gameTime)
        {
            //
            //
            base.Tick(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //
            //
            base.Draw(gameTime);
        }
        #endregion
    }
}
