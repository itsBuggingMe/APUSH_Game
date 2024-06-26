﻿using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.Interface.Elements
{
    internal class SelectorButton : BoundedGui
    {
        private Rectangle? source;
        private Color color;
        private Texture2D texture;
        private float juice;
        private Action onClicked;
        private Action onEnter;
        private bool LastFrameState;
        public SelectorButton(string txtre, Rectangle dest, Rectangle? source, Color color, Action OnClicked = null, Action onEnter = null) : base(dest)
        {
            this.onEnter = onEnter;
            this.onClicked = OnClicked;
            texture = GameRoot.Game.Content.Load<Texture2D>(txtre);
            this.color = color;
            this.source = source;
        }

        public override void Tick(GameTime gameTime)
        {
            if (Bounds.Contains(InputHelper.MouseLocation))
            {
                if (!LastFrameState)
                    onEnter?.Invoke();
                juice -= 2f;
                if (InputHelper.FallingEdge(MouseButton.Left))
                    onClicked?.Invoke();
                if (InputHelper.Down(MouseButton.Left))
                    juice += 4;
            }
            juice -= 0.4f * juice;
            LastFrameState = Bounds.Contains(InputHelper.MouseLocation);
            base.Tick(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            int scaledJuice = (int)(juice * ScaleVector.Length());
            Rectangle cpy = Bounds;
            //cpy.Inflate(scaledJuice, scaledJuice);
            Globals.SpriteBatch.Draw(texture, cpy.OffsetCopy(new Point(scaledJuice)), source, color);
            base.Draw(gameTime);
        }
    }
}
