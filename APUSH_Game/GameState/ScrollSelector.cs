using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class ScrollSelector : IGameObject
    {
        private Action<int> whenSelected;
        private int min;
        private int max;
        private float current;
        private float currentVel;
        private PromptBox bg;
        private GameWorld world;
        public ScrollSelector(string text, Action<int> whenSelected, int min, int max, GameWorld world)
        {
            this.world = world;
            world.ScrollLock = true;
            current = 0;
            bg = new PromptBox(text, new Vector2(0.5f, 0.8f), false);
            this.whenSelected = whenSelected;
            this.min = min;
            this.max = max;
        }

        public bool Delete { get; set; }

        public void Update()
        {
            if(InputHelper.DeltaScroll != 0)
                currentVel += (InputHelper.DeltaScroll > 0 ? -1 : 1) * -0.1f;
            if (InputHelper.Down(Keys.Left))
                currentVel += 0.02f;
            if (InputHelper.Down(Keys.Right))
                currentVel -= 0.02f;

            current += currentVel;
            currentVel *= 0.9f;

            current = Math.Clamp(current, min, max);
            if(currentVel < 0.01f)
            {
                float closest = MathF.Round(current);
                current += (closest - current) * 0.05f;
            }

            if(InputHelper.RisingEdge(Keys.Enter) || InputHelper.RisingEdge(MouseButton.Left))
            {
                whenSelected?.Invoke((int)Math.Round(current));
                Delete = true;
                world.ScrollLock = false;
            }
        }

        public void Draw()
        {
            bg.Draw();
            GameRoot.Game.PostDraw += DoDraw;
        }

        private void DoDraw(GameTime gameTime)
        {
            Vector2 origin = new Vector2(0.5f, 0.8f) * Globals.WindowSize.V() - (Vector2.UnitX * 50 * current);
            for(int i = min; i <= max; i++)
            {
                float opacity = Math.Abs(i - current) * 0.8f;
                opacity *= opacity;
                Globals.SpriteBatch.DrawStringCentered(i.ToString(), origin + Vector2.UnitX * i * 50, Globals.Scale * 0.2f, Color.White *  (1 - Math.Max(opacity, 0)));
            }
        }
    }
}
