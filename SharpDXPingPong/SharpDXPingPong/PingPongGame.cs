using System.Windows.Forms;
using Engine;
using SharpDX;
using SharpDXPingPong.Components;

namespace SharpDXPingPong
{
    internal class PingPongGame : Game
    {
        private readonly Camera _camera;
        private readonly CameraController _cameraController;
        
        internal PingPongGame(string name, int width = 800, int height = 600) : base(name, width, height)
        {
            _camera = new Camera(this);
            _cameraController = new CameraController(this, _camera)
            {
                CameraPosition = new Vector3(0, 0, 300),
//                VelocityMagnitude = 0f,
//                MouseSensitivity = 0f
            };
        }

        protected override void Init()
        {
            Components.Add(new PlaneComponent(this, "Proj.hlsl", "Proj.hlsl", _camera));
            Components.Add(new PadComponent(this, "Shader.hlsl", "Shader.hlsl", _camera));
            base.Init();
        }

        protected override void Update(float deltaTime)
        {
            _cameraController.Update(deltaTime);

            if (InputDevice.IsKeyDown(Keys.F11))
            {
                SwapChain.GetFullscreenState(out var isFullscreen, out var _);
                SwapChain.SetFullscreenState(!isFullscreen, null);
            }

            if (InputDevice.IsKeyDown(Keys.Escape))
            {
                Shutdown();
            }

            base.Update(deltaTime);
        }
    }
}