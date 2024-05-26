using APUSH_Game.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apos.Camera
{
    public class DefaultViewport : SplitViewport
    {
        public DefaultViewport(GraphicsDevice graphicsDevice, GameWindow window) : base(graphicsDevice, window, 0f, 0f, 1f, 1f) { }
    }
    public interface IVirtualViewport : IDisposable
    {
        int X { get; }
        int Y { get; }
        int Width { get; }
        int Height { get; }

        Vector2 XY { get; }

        Vector2 Origin { get; }

        float VirtualWidth { get; }
        float VirtualHeight { get; }

        Matrix Transform(Matrix view);
        void Set();
        void Reset();
    }

    public class Camera
    {
        public Camera(IVirtualViewport virtualViewport)
        {
            VirtualViewport = virtualViewport;
        }

        public float X
        {
            get => _xy.X;
            set
            {
                _xy.X = value;
                _xyz.X = value;
            }
        }
        public float Y
        {
            get => _xy.Y;
            set
            {
                _xy.Y = value;
                _xyz.Y = value;
            }
        }
        public float Z
        {
            get => _xyz.Z;
            set
            {
                _xyz.Z = value;
            }
        }

        public float FocalLength
        {
            get => _focalLength;
            set
            {
                _focalLength = value > 0.01f ? value : 0.01f;
            }
        }

        public float Rotation { get; set; } = 0f;
        public Vector2 Scale { get; set; } = Vector2.One;

        public Vector2 XY
        {
            get => _xy;
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
        public Vector3 XYZ
        {
            get => _xyz;
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        public IVirtualViewport VirtualViewport
        {
            get;
            set;
        }

        public void SetViewport()
        {
            VirtualViewport.Set();
        }
        public void ResetViewport()
        {
            VirtualViewport.Reset();
        }

        public Matrix View => GetView(0);
        public Matrix ViewInvert => GetViewInvert(0);

        public Matrix GetView(float z = 0)
        {
            float scaleZ = ZToScale(_xyz.Z, z);
            return VirtualViewport.Transform(
                // Matrix.CreateTranslation(new Vector3(-VirtualViewport.Origin, 0f)) * // This makes the camera position be at the top left
                Matrix.CreateTranslation(new Vector3(-XY, 0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Scale.X, Scale.Y, 1f) *
                Matrix.CreateScale(scaleZ, scaleZ, 1f) *
                Matrix.CreateTranslation(new Vector3(VirtualViewport.Origin, 0f)));
        }

        public Matrix GetView3D()
        {
            return
                Matrix.CreateLookAt(XYZ, new Vector3(XY, Z - 1), new Vector3((float)Math.Sin(Rotation), (float)Math.Cos(Rotation), 0)) *
                Matrix.CreateScale(Scale.X, -Scale.Y, 1f);
        }
        public Matrix GetViewInvert(float z = 0) => Matrix.Invert(GetView(z));

        public Matrix GetProjection()
        {
            return Matrix.CreateOrthographicOffCenter(0, VirtualViewport.Width, VirtualViewport.Height, 0, 0, 1);
        }
        public Matrix GetProjection3D(float nearPlaneDistance = 0.01f, float farPlaneDistance = 100f)
        {
            var aspect = VirtualViewport.VirtualWidth / (float)VirtualViewport.VirtualHeight;
            var fov = (float)Math.Atan(VirtualViewport.VirtualHeight / 2f / FocalLength) * 2f;

            return Matrix.CreatePerspectiveFieldOfView(fov, aspect, nearPlaneDistance, farPlaneDistance);
        }

        public float ScaleToZ(float scale, float targetZ)
        {
            if (scale == 0)
            {
                return float.MaxValue;
            }
            return FocalLength / scale + targetZ;
        }
        public float ZToScale(float z, float targetZ)
        {
            if (z - targetZ == 0)
            {
                return float.MaxValue;
            }
            return FocalLength / (z - targetZ);
        }

        public float WorldToScreenScale(float z = 0f) => Vector2.Distance(WorldToScreen(0f, 0f, z), WorldToScreen(1f, 0f, z));
        public float ScreenToWorldScale(float z = 0f) => Vector2.Distance(ScreenToWorld(0f, 0f, z), ScreenToWorld(1f, 0f, z));

        public Vector2 WorldToScreen(float x, float y, float z = 0f) => WorldToScreen(new Vector2(x, y), z);
        public Vector2 WorldToScreen(Vector2 xy, float z = 0f)
        {
            return Vector2.Transform(xy, GetView(z)) + VirtualViewport.XY;
        }
        public Vector2 ScreenToWorld(float x, float y, float z = 0f) => ScreenToWorld(new Vector2(x, y), z);
        public Vector2 ScreenToWorld(Vector2 xy, float z = 0f)
        {
            return Vector2.Transform(xy - VirtualViewport.XY, GetViewInvert(z));
        }

        public bool IsZVisible(float z, float minDistance = 0.1f)
        {
            float scaleZ = ZToScale(Z, z);
            float maxScale = ZToScale(minDistance, 0f);

            return scaleZ > 0 && scaleZ < maxScale;
        }
        public BoundingFrustum GetBoundingFrustum(float z = 0)
        {
            // TODO: Use 3D view and projection?
            Matrix view = GetView(z);
            Matrix projection = GetProjection();
            return new BoundingFrustum(view * projection);
        }

        private Vector2 _xy = Vector2.Zero;
        private Vector3 _xyz = new Vector3(Vector2.Zero, 1f);
        private float _focalLength = 1f;
    }

    public class SplitViewport : IVirtualViewport
    {
        public SplitViewport(GraphicsDevice graphicsDevice, GameWindow window, float left, float top, float right, float bottom)
        {
            _graphicsDevice = graphicsDevice;

            _left = left;
            _top = top;
            _right = right;
            _bottom = bottom;

            _window = window;
            Globals.WindowSizeEvent += (p) => OnClientSizeChanged(null, null);
            OnClientSizeChanged(null, null);
        }

        public void Dispose()
        {
            _window.ClientSizeChanged -= OnClientSizeChanged;
        }

        public int X => _viewport.X;
        public int Y => _viewport.Y;
        public int Width => _viewport.Width;
        public int Height => _viewport.Height;

        public Vector2 XY => new Vector2(X, Y);

        public Vector2 Origin => _origin;

        public float VirtualWidth => Width;
        public float VirtualHeight => Height;

        public void Set()
        {
            _oldViewport = _graphicsDevice.Viewport;
            _graphicsDevice.Viewport = _viewport;
        }
        public void Reset()
        {
            _graphicsDevice.Viewport = _oldViewport;
        }

        public Matrix Transform(Matrix view)
        {
            return view;
        }

        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            int gWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
            int gHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;

            _viewport = new Viewport((int)(gWidth * _left), (int)(gHeight * _top), (int)(gWidth * (_right - _left)), (int)(gHeight * (_bottom - _top)));

            _origin = new Vector2(_viewport.Width / 2f, _viewport.Height / 2f);
        }

        public void UpdateViewport()
        {
            OnClientSizeChanged(null, null);
        }

        private GraphicsDevice _graphicsDevice;
        private GameWindow _window;
        private Viewport _viewport;
        private Viewport _oldViewport;
        private Vector2 _origin;

        private float _left;
        private float _top;
        private float _right;
        private float _bottom;
    }
}
