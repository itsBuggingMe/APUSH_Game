using APUSH_Game.GameState;
using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game
{
    internal class StartState : IScreen
    {
        private List<PromptBox> obj = new();

        private PromptBox[] Sequence;
        private int Current = 2;
        private bool moving;
        private MainGameState gs = new();

        public StartState()
        {
            Sequence = new string[]
            {
                "The year is 1860, and Civil War is in \n" +
                "the air. The North's goal is to capture\n" +
                "every southern territory. The South's\n" +
                "goal is to survive 20 turns and force the" +
                "\nNorth to sue for peace.",
                "Each player has a certain amount of Politcal\n" +
                "Capital and Money. Money is used to deploy\n" +
                "troops, and Political Capital is used to\n" +
                " move and continue your turn. Without\n" +
                "Political Capital, your turn is over. You can\n" +
                "also press space to move to the next turn.",
                "Each turn is composed of actions, of which\n" +
                "are three types. Move, Deploy, and Attack.\n" +
                "These actions can be selected from the\n" +
                "right hand side.",
                "Move: You can only move to adjacent\n" +
                "territories, and it costs one Political Captial\n" +
                "Deploy: Each troop desployed costs one\n" +
                "Political Capital\n" +
                "Attack: Can result in a win, loss, or\n" +
                "draw where a win gains Political Capital,\n" +
                "and otherwise loses Political Captial."
            }.Select(s => new PromptBox(s.Contains('\n') ? s : GameWorld
                    .RebuildSentence(s),
                    new Vector2(0.5f, 0.4f), false, null, 0.8f)).ToArray();
                
            for(int i = 0; i < Sequence.Length; i++)
            {
                Sequence[i].Tag = i;
                obj.Add(Sequence[i]);
                obj.Add(new PromptBox("Next", new Vector2(0.5f, 0.8f), true, MoveNext)
                {
                    Tag = i
                });
            }

            MoveNext();
        }

        private void MoveNext()
        {
            if (moving)
                return;
            moving = true;
            AnimationPool.Instance.Request().Reset(
                SetHorizontalValue, Current, () => moving = false, new KeyFrame(Current - 1, 60, AnimationType.EaseInOutQuad));
            Current--;

            if(Current == -Sequence.Length + 1)
            {
                GameWorld.FadeTransition(gs);
            }
        }

        private void SetHorizontalValue(float offset)
        {
            Debug.WriteLine(offset);
            offset -= 0.5f;
            for (int i = 0; i < obj.Count; i++)
            {
                PromptBox b = obj[i];
                int index = (int)b.Tag;

                b.SetPosition(new Vector2(offset + index, b.WindowPosition.Y));
            }
        }

        public void Tick(GameTime Gametime)
        {
            for (int i = 0; i < obj.Count; i++)
                obj[i].Update();
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
        public void OnStateEnter(IScreen previous, object transferInfo)
        {

        }

        public object OnStateExit()
        {
            return null;
        }
    }

}
