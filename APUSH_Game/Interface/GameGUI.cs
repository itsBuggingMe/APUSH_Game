using APUSH_Game.GameState;
using APUSH_Game.Helpers;
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
                moneyText.Text = value.ToString();
            }
        }
        private TextElement moneyText;

        private TextElement[] Troops = new TextElement[3];

        private int _numCavalry;
        public int NumCavalry
        {
            get => _numCavalry;
            set
            {
                _numCavalry = value;
                cavalryText.Text = value.ToString();
            }
        }
        private TextElement cavalryText => Troops[2];

        private int _numArtillery;
        public int NumArtillery
        {
            get => _numArtillery;
            set
            {
                _numArtillery = value;
                artilleryText.Text = value.ToString();
            }
        }
        private TextElement artilleryText => Troops[1];

        private int _numInfantry;
        public int NumInfantry
        {
            get => _numInfantry;
            set
            {
                _numInfantry = value;
                infantryText.Text = value.ToString();
            }
        }
        private TextElement infantryText => Troops[0];

        private int _numPoliticalCapital;
        public int NumPoliticalCapital
        {
            get => _numPoliticalCapital;
            set
            {
                _numPoliticalCapital = value;
                politicalCapitalText.Text = value.ToString();
            }
        }
        private TextElement politicalCapitalText;

        private Color color;

        private GuiBase LeftPanel;
        private GuiBase RightPanel;
        public TurnAction CurrentAction { get; private set; }
        public GameGUI(bool side) : base(Vector2.Zero)
        {
            color = side ? RegionObject.UnionState : RegionObject.ConfederateState;
            color *= 1.2f;
            LeftPanel = AddElement(new GuiBase());
            RightPanel = AddElement(new GuiBase());

            moneyText = LeftPanel.AddElement(new TextElement(new Vector2(120, 105), "0", Color.Black, 0.7f, ElementAlign.LeftMiddle));
            LeftPanel.AddElement(new Image(new Vector2(70, 100), Color.Black, new Vector2(0.3f), ElementAlign.Center, "MoneySymbol"));

            for(int i = 0; i < Troop.Sources.Length; i++)
            {
                LeftPanel.AddElement(new TextElement(new Vector2(100, 200 + i * 150), Troop.Names[i], Color.Black, 0.25f, ElementAlign.Center));
                LeftPanel.AddElement(new Image(new Vector2(100, 225 + i * 150), Color.White, new(0.4f), ElementAlign.TopMiddle, "NatoIcon", Troop.Sources[i]));
                Troops[i] = LeftPanel.AddElement(new TextElement(new Vector2(185  , 278 + i * 150), "0", Color.Black, 0.5f, ElementAlign.LeftMiddle));
            }

            LeftPanel.AddElement(new TextElement(new Vector2(188, 830), "You Are:", Color.Black, 0.4f, ElementAlign.Center));
            LeftPanel.AddElement(new Image(new Vector2(180, 950), Color.White, Vector2.One, ElementAlign.Center, "Flags", 
                side ? new Rectangle(0, 0, 255, 142) : new Rectangle(256, 0, 255, 142)));

            Vector2 pcOffset = new Vector2(75, -90);
            RightPanel.AddElement(new TextElement(new Vector2(1700, 200) + pcOffset, "Political\nCapital:", Color.Black, 0.25f, ElementAlign.Center));
            RightPanel.AddElement(new Image(new Vector2(1700, 300) + pcOffset, Color.Lerp(color, Color.Black, 0.5f), new(0.3f), ElementAlign.Center, "pcICON"));
            politicalCapitalText = RightPanel.AddElement(new TextElement(new Vector2(1780, 205) + pcOffset, "0", Color.Black, 0.5f, ElementAlign.LeftMiddle));

            RightPanel.AddElement(new TextElement(new Vector2(1780, 350), "Actions", Color.Black, 0.4f, ElementAlign.BottomMiddle));
            Color dc = Color.Lerp(color, Color.Black, 0.7f);

            var box = RightPanel.AddElement(new SelectorButton("ActIcons", new Rectangle(1720, 370, 128, 128), new Rectangle(0, 384, 128, 128), Color.White));

            RightPanel.AddElement(new SelectorButton("ActIcons", new Rectangle(1720, 370, 128, 128), new Rectangle(0,0,128,128), dc, () => MoveObj(0)));
            RightPanel.AddElement(new SelectorButton("ActIcons", new Rectangle(1720, 370 + 160, 128, 128), new Rectangle(0,128,128,128), dc, () => MoveObj(1)));
            RightPanel.AddElement(new SelectorButton("ActIcons", new Rectangle(1720, 370 + 320, 128, 128), new Rectangle(0,256,128,128), dc, () => MoveObj(2)));

            void MoveObj(int callFrom)
            {
                Vector2 currentLoc = box.LocalLocationUnscale;
                AnimationPool.Instance.Request().Reset(
                    f => box.LocalLocationUnscale = Vector2.Lerp(currentLoc, new Vector2(1720, 370 + callFrom * 160),f),
                    0, null, new KeyFrame(1, 20, AnimationType.EaseInOutExpo));
                CurrentAction = (TurnAction)callFrom;
            }

            NumPoliticalCapital = 6;
        }

        private const int AnimationDistance = 400;
        public Animation AnimateOut()
        {
            return AnimationPool.Instance.Request().Reset(
                f =>
                {
                    LeftPanel.LocalLocationUnscale = Vector2.Lerp(Vector2.Zero, -AnimationDistance * Vector2.UnitX, f);
                    RightPanel.LocalLocationUnscale = Vector2.Lerp(Vector2.Zero, AnimationDistance * Vector2.UnitX, f);
                },
                0, null, new KeyFrame(1, 30, AnimationType.Parabolic));
        }

        public Animation AnimateIn()
        {
            LeftPanel.LocalLocationUnscale = -AnimationDistance * Vector2.UnitX;
            RightPanel.LocalLocationUnscale = AnimationDistance * Vector2.UnitX;
            return AnimationPool.Instance.Request().Reset(
                f =>
                {
                    LeftPanel.LocalLocationUnscale = Vector2.Lerp(Vector2.Zero, -AnimationDistance * Vector2.UnitX, 1 - f);
                    RightPanel.LocalLocationUnscale = Vector2.Lerp(Vector2.Zero, AnimationDistance * Vector2.UnitX, 1 - f);
                },
                0, null, new KeyFrame(1, 30, AnimationType.InverseParabolic));
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

    internal enum TurnAction
    {
        Move = 0,
        Deploy = 1,
        Attack = 2,
    }
}
