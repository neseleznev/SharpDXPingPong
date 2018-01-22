using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;
using SharpDX;
using SharpDXPingPong.Components;

namespace SharpDXPingPong.Controllers
{
    class PadController
    {
        private readonly PadComponent _pad;
        private readonly InputDevice _inputDevice;
        private readonly Game _game;
        private int _series;
        private short _previousDirection;

        internal PadController(PadComponent pad, InputDevice inputDevice, Game game)
        {
            _pad = pad;
            _inputDevice = inputDevice;
            _game = game;
        }

        public void Reset()
        {
            _pad.Reset();
            _series = 0;
        }

        internal void Update(float deltaTime)
        {
            short direction = 0;
            if (_inputDevice.IsKeyDown(Keys.A))
                direction = -1;
            if (_inputDevice.IsKeyDown(Keys.D))
                direction = 1;
            
            _pad.Position.X += direction * _pad.Velocity * deltaTime;
            _pad.Position.X = Math.Min(_game.GetWidth() - _pad.Width, Math.Max(0, _pad.Position.X));
            
            if (_previousDirection == direction)
            {
                _series += direction;
            }
            else
            {
                _series = 0;
            }

            _previousDirection = direction;
        }

        public float GetAcceleration()
        {
            return Math.Max(0.25f, Math.Min(1.75f, 1.0f + _series / 50.0f));
        }

        public bool IsTouching(float x, float y)
        {
            var padTop = _pad.Position.Y + _pad.Height;
            return _pad.Position.X < x && x < _pad.Position.X + _pad.Width
                                       && padTop - 5 < y && y < padTop + 2;
        }

        public float GetPadStopLine()
        {
            return _pad.Position.Y - _pad.Height;
        }

        public void IncreaseSpeed()
        {
            _pad.Velocity = _pad.Velocity * 1.25f;
        }

        public void DecreaseSpeed()
        {
            _pad.Velocity = _pad.Velocity / 1.25f;
        }

    }
}
