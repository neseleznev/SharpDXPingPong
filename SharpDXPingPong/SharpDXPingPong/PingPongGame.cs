using System;
using System.Windows.Forms;
using Engine;
using SharpDX;
using SharpDXPingPong.Components;
using SharpDXPingPong.Components.WarpZone;
using SharpDXPingPong.Controllers;

namespace SharpDXPingPong
{
    internal class PingPongGame : Game
    {
        private readonly Camera _camera;
        private readonly CameraController _cameraController;
        private PadController _pad;
        private BallComponent _ball;
        private WarpComponent _speedUp;
        private WarpComponent _speedDown;
        private readonly Random _random;
        private bool _isGameOver = true;

        internal PingPongGame(string name, int width = 800, int height = 600) : base(name, width, height)
        {
            _camera = new Camera(this);
            _cameraController = new CameraController(this, _camera)
            {
                CameraPosition = new Vector3(0, 0, 120),
                VelocityMagnitude = 0f,  // Comment these lines to get FPS
                MouseSensitivity = 0f  // in the background
            };
            _random = new Random();
        }

        protected override void Init()
        {
            Components.Add(new PlaneComponent(this, "Proj.hlsl", "Proj.hlsl", _camera));

            var padComponent = new PadComponent(this, "Proj.hlsl", "Proj.hlsl", _camera);
            _pad = new PadController(padComponent, InputDevice, this);
            Components.Add(padComponent);

            _ball = new BallComponent(this, "Proj.hlsl", "Proj.hlsl", _camera);
            Components.Add(_ball);

//            AddPsychedelicBackground();

            _speedUp = new BallSpeedUp(this, "Proj.hlsl", "Shader.hlsl", _camera);
            Components.Add(_speedUp);
            _speedDown = new BallSpeedDown(this, "Proj.hlsl", "Shader.hlsl", _camera);
            Components.Add(_speedDown);
            base.Init();
        }

        private void AddPsychedelicBackground()
        {
            const float step = 10.0f;
            for (var i = -3; i <= 4; i++)
            {
                for (var j = -3; j <= 4; ++j)
                {
                    if ((i + 16) % 2 == (j + 16) % 2)
                    {
                        Components.Add(new TexturedQuadComponent(this, "acid.jpg", _camera,
                            new Vector3(i * step - 5, j * step - 5, (i * i + j * j * 3) % 3), 5.0f));
                    }
                }
            }

            Components.Add(new TexturedQuadComponent(this, "pad.jpg", _camera,
                new Vector3(0, -90, 0.1f), 50f));
            Components.Add(new TexturedQuadComponent(this, "pad.jpg", _camera,
                new Vector3(0, 90, 0.2f), 50f));
            Components.Add(new TexturedQuadComponent(this, "pad.jpg", _camera,
                new Vector3(-90, 0, 0.3f), 50f));
            Components.Add(new TexturedQuadComponent(this, "pad.jpg", _camera,
                new Vector3(90, 0, 0.4f), 50f));
        }

        protected override void Update(float deltaTime)
        {
            if (InputDevice.IsKeyDown(Keys.F2))
            {
                StartNewGame();
            }

            if (InputDevice.IsKeyDown(Keys.F11))
            {
                SwapChain.GetFullscreenState(out var isFullscreen, out var _);
                SwapChain.SetFullscreenState(!isFullscreen, null);
            }

            if (InputDevice.IsKeyDown(Keys.Escape))
            {
                Shutdown();
            }

            _cameraController.Update(deltaTime);

            if (!_isGameOver)
            {
                _pad.Update(deltaTime);
                UpdateBallVsPad();
                UpdateSpeedBalls(deltaTime);
            }

            base.Update(deltaTime);
        }

        private void UpdateBallVsPad()
        {
            var newX = _ball.Position.X + _ball.HorizontalDirection * _ball.Velocity;
            var newY = _ball.Position.Y + _ball.VerticalDirection * _ball.Velocity;

            var isTouching = _pad.IsTouching(newX, newY - _ball.Radius);
            if (isTouching)
            {
                _ball.VerticalDirection = 1;
                _ball.VerticalDirection *= _pad.GetAcceleration();
            }
            else if (newY < _pad.GetPadStopLine())
            {
                Console.WriteLine("Game over!");
                GameOver();
            }
            else if (newY + _ball.Radius > GetHeight())
            {
                _ball.VerticalDirection *= -1;
            }

            if (newX - _ball.Radius < 0 || newX + _ball.Radius > GetWidth())
            {
                _ball.HorizontalDirection *= -1;
            }

            _ball.Position.X += _ball.HorizontalDirection * _ball.Velocity;
            _ball.Position.Y += _ball.VerticalDirection * _ball.Velocity;
        }
        
        private void UpdateSpeedBalls(float deltaTime)
        {
            _speedUp.Position.Y = _speedUp.Position.Y - deltaTime * _speedUp.Velocity;
            _speedDown.Position.Y = _speedDown.Position.Y - deltaTime * _speedDown.Velocity;

            if (_random.NextDouble() < 0.0001)
            {
                _speedUp.DropRandomly();
            }

            if (_random.NextDouble() < 0.0001)
            {
                _speedDown.DropRandomly();
            }

            if (_pad.IsTouching(_speedUp.Position.X, _speedUp.Position.Y - _speedUp.Radius))
            {
                _speedUp.Hide();
                _ball.IncreaseSpeed();
            }
            else if (_speedUp.Position.Y - _speedUp.Radius < 0)
            {
                _speedUp.Hide();
            }

            if (_pad.IsTouching(_speedDown.Position.X, _speedDown.Position.Y - _speedDown.Radius))
            {
                _speedDown.Hide();
                _ball.DecreaseSpeed();
            }
            else if (_speedDown.Position.Y - _speedDown.Radius < 0)
            {
                _speedDown.Hide();
            }
        }

        public void GameOver()
        {
            _isGameOver = true;
        }

        public void StartNewGame()
        {
            _isGameOver = false;
            _ball.Reset();
            _pad.Reset();
        }
    }
}