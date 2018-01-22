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
        private WarpComponent _longPad;
        private WarpComponent _speedUp;
        private WarpComponent _speedDown;
        private readonly Random _random;
        private bool _isGameOver;

        internal PingPongGame(string name, int width = 800, int height = 600) : base(name, width, height)
        {
            _camera = new Camera(this);
            _cameraController = new CameraController(this, _camera)
            {
                CameraPosition = new Vector3(0, 0, 150),
//                VelocityMagnitude = 0f,
//                MouseSensitivity = 0f
            };
            _random = new Random();
        }

        protected override void Init()
        {
            Components.Add(new PlaneComponent(this, "Proj.hlsl", "Proj.hlsl", _camera));

            var padComponent = new PadComponent(this, "Shader.hlsl", "Shader.hlsl", _camera);
            _pad = new PadController(padComponent, InputDevice);
            Components.Add(padComponent);

            _ball = new BallComponent(this, "Shader.hlsl", "Shader.hlsl", _camera);
            Components.Add(_ball);

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

            _longPad = new LongPad(this, "Shader.hlsl", "Shader.hlsl", _camera);
            Components.Add(_longPad);
            _speedUp = new BallSpeedUp(this, "Shader.hlsl", "Shader.hlsl", _camera);
            Components.Add(_speedUp);
            _speedDown = new BallSpeedDown(this, "Shader.hlsl", "Shader.hlsl", _camera);
            Components.Add(_speedDown);
            base.Init();
        }

        protected override void Update(float deltaTime)
        {
            if (InputDevice.IsKeyDown(Keys.F2))
            {
                StartNewGame();
            }

            _cameraController.Update(deltaTime);

            if (_isGameOver)
            {
                return;
            }

            _pad.Update(deltaTime);

            #region Ball vs Pad
            var newX = _ball.Center.X + _ball.horizontalDirection * _ball.Velocity;
            var newY = _ball.Center.Y + _ball.verticalDirection * _ball.Velocity;

            var isTouching = _pad.IsTouching(newX, newY - _ball.Radius.Y);
            if (isTouching)
            {
                _ball.verticalDirection = 1;
                _ball.verticalDirection *= _pad.GetAcceleration();
            } else if (newY < _pad.GetPadStopLine())
            {
                Console.WriteLine("Game over!");
                GameOver();
            }
            else if (newY + _ball.Radius.Y > 1.0f)
            {
                _ball.verticalDirection *= -1;
            }
            if (newX - _ball.Radius.X < -1.0f || newX + _ball.Radius.X > 1.0f)
            {
                _ball.horizontalDirection *= -1;
            }

            _ball.Center.X = _ball.Center.X + _ball.horizontalDirection * _ball.Velocity;
            _ball.Center.Y = _ball.Center.Y + _ball.verticalDirection * _ball.Velocity;
            #endregion

            #region LongPad
            if (_random.NextDouble() < 0.0001)
            {
                _longPad.DropRandomly();
            }

            if (_pad.IsTouching(_longPad.Center.X, _longPad.Center.Y - _longPad.Radius.Y))
            {
                _longPad.Hide();
                _pad.IncreaseWidth();
            } else if (_longPad.Center.Y - _longPad.Radius.Y + _ball.Radius.Y > 1.0f)
            {
                _longPad.Hide();
            }
            #endregion

            #region BallSpeed Up/Down
            if (_random.NextDouble() < 0.0001)
            {
                _speedUp.DropRandomly();
            }
            if (_random.NextDouble() < 0.0001)
            {
                _speedDown.DropRandomly();
            }

            if (_pad.IsTouching(_speedUp.Center.X, _speedUp.Center.Y - _speedUp.Radius.Y))
            {
                _speedUp.Hide();
                _ball.IncreaseSpeed();
            }
            else if (_speedUp.Center.Y - _speedUp.Radius.Y + _ball.Radius.Y > 1.0f)
            {
                _speedUp.Hide();
            }

            if (_pad.IsTouching(_speedDown.Center.X, _speedDown.Center.Y - _speedDown.Radius.Y))
            {
                _speedDown.Hide();
                _ball.DecreaseSpeed();
            }
            else if (_speedDown.Center.Y - _speedDown.Radius.Y + _ball.Radius.Y > 1.0f)
            {
                _speedDown.Hide();
            }

            #endregion
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