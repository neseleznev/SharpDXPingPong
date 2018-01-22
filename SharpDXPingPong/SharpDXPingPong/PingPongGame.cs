using System;
using System.Windows.Forms;
using Engine;
using SharpDX;
using SharpDXPingPong.Components;
using SharpDXPingPong.Controllers;

namespace SharpDXPingPong
{
    internal class PingPongGame : Game
    {
        private readonly Camera _camera;
        private readonly CameraController _cameraController;
        private PadController pad;
        private BallComponent ball;
        
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

            var padComponent = new PadComponent(this, "Shader.hlsl", "Shader.hlsl", _camera);
            pad = new PadController(padComponent, InputDevice);
            Components.Add(padComponent);

            ball = new BallComponent(this, "Shader.hlsl", "Shader.hlsl", _camera);
            Components.Add(ball);
            base.Init();
        }

        protected override void Update(float deltaTime)
        {
            _cameraController.Update(deltaTime);

            pad.Update(deltaTime);

            var newX = ball.Center.X + ball.horizontalDirection * ball.Velocity;
            var newY = ball.Center.Y + ball.verticalDirection * ball.Velocity;

            var isTouching = pad.IsTouching(newX, newY - ball.Radius.Y);
            if (isTouching)
            {
                ball.verticalDirection = 1;
                ball.verticalDirection *= pad.GetAcceleration();
            }
            else if (newY + ball.Radius.Y > 1.0f)
            {
                ball.verticalDirection *= -1;
            }
            if (newX - ball.Radius.X < -1.0f || newX + ball.Radius.X > 1.0f)
            {
                ball.horizontalDirection *= -1;
            }

            ball.Center.X = ball.Center.X + ball.horizontalDirection * ball.Velocity;
            ball.Center.Y = ball.Center.Y + ball.verticalDirection * ball.Velocity;

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