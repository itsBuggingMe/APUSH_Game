using APUSH_Game.GameState;
using APUSH_Game.Helpers;
using APUSH_Game.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game
{
    internal class EndState : IScreen
    {
        private List<IGameObject> obj = new();

        public EndState(bool winner, int turns)
        {
            if(winner)
                obj.Add(new PromptBox(GameWorld
                    .RebuildSentence($"The Union was able to force the Confederacy to surrender after {turns} turns. The Union was preserved!"),
                    new Vector2(0.5f), false));
            else
                obj.Add(new PromptBox(GameWorld
                    .RebuildSentence($"With the growing unpopularity of the war after 20, the Union is forced to sue for peace. The US will remain separated."),
                    new Vector2(0.5f), false));

            obj.Add(new PromptBox("Back to Start", new Vector2(0.5f, 0.8f), true, () => ScreenManager.Instance.ChangeState(new StartState())));
            AnimationPool.Instance.Request().Reset(f =>
            {
                GameRoot.Game.PostDraw += g =>
                {
                    Globals.SpriteBatch.Draw(Globals.Pixel, new Rectangle(Point.Zero, Globals.WindowSize), Color.Black * f);
                };
            }, 1, null, new KeyFrame(0, 120, AnimationType.EaseInOutQuart));
        }

        public void Tick(GameTime Gametime)
        {
            foreach(var obj in obj)
            {
                obj.Update();
            }
        }

        public void Draw(GameTime Gametime)
        {
            Globals.Graphics.Clear(new Color(0, 162, 232));

            Globals.SpriteBatch.Begin();
            GameRoot.Game.PostDraw?.Invoke(Gametime);
            GameRoot.Game.PostDraw = null;
            foreach (var obj in obj)
                obj.Draw();
            Globals.SpriteBatch.End();
        }

        public object OnStateExit() => null;
        public void OnStateEnter(IScreen previous, object transferInfo) { }
    }

    internal class StartState : IScreen
    {
        public void Draw(GameTime Gametime)
        {

        }

        public void OnStateEnter(IScreen previous, object transferInfo)
        {

        }

        public object OnStateExit()
        {
            return null;
        }

        public void Tick(GameTime Gametime)
        {

        }
    }
}
