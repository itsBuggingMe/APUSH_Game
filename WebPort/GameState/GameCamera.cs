using Apos.Camera;
using APUSH_Game.Helpers;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APUSH_Game.GameState
{
    internal class GameCamera
    {
        public float ScreenShake { get; set; }

        public readonly Camera _camera;
        private Vector2 _Location;
        public Vector2 Location
        {
            get => _Location;
            set
            {
                _camera.XY = value;
                _Location = value;
            }
        }

        public float ZoomM { get; set; } = 1;

        private float _zoom;
        public float Zoom
        {
            get => _zoom;
            set
            {
                _camera.Scale = new Vector2(value * ZoomM);
                _zoom = value;
            }
        }

        public GameCamera()
        {
            IVirtualViewport defaultViewport = new SplitViewport(Globals.Graphics, GameRoot.Game.Window, 0, 0, 1, 1);
            _camera = new Camera(defaultViewport);
            _zoom = 1;
            SetPosition(Vector2.Zero);
        }

        public Vector2 WorldToScreen(Vector2 world) => _camera.WorldToScreen(world);
        public Vector2 ScreenToWorld(Vector2 screen) => _camera.ScreenToWorld(screen);
        public void Update(GameTime gameTime) => ScreenShake -= gameTime.ElapsedGameTime.Milliseconds;
        public void Set() => _camera.SetViewport();
        public void StartSpriteBatch(SpriteBatch spriteBatch)
        {
            if(ScreenShake > 0)
            {
                Vector2 p = Location;
                Location += new Vector2(Random.Shared.NextSingle(-1,1), Random.Shared.NextSingle(-1,1)) * 8f / Zoom;
                spriteBatch.Begin(transformMatrix: _camera.View);
                Location = p;
            }
            else
            {
                spriteBatch.Begin(transformMatrix: _camera.View);
            }
        }
        public void Reset() => _camera.ResetViewport();
        public void SetPosition(Vector2 vector2) => Location = vector2;
        public void SetZoom(float zoom) => Zoom = zoom;
    }

}
