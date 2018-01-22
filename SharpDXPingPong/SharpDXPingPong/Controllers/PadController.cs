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
        private int _series;
        private short _previousDirection;

        internal PadController(PadComponent pad, InputDevice inputDevice)
        {
            _pad = pad;
            _inputDevice = inputDevice;
        }

        internal void Update(float deltaTime)
        {
            short direction = 0;
            if (_inputDevice.IsKeyDown(Keys.A))
                direction = -1;
            if (_inputDevice.IsKeyDown(Keys.D))
                direction = 1;
            _pad.Center.X += direction * _pad.Velocity * deltaTime;
            _pad.Center.X = Math.Min(1 - _pad.Width / 2, Math.Max(-1 + _pad.Width / 2, _pad.Center.X));
            
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
            return Math.Max(0.25f, Math.Min(1.75f, 1.0f + _series / 100.0f));
        }

        public bool IsTouching(float x, float y)
        {
            var leftX = _pad.Center.X - _pad.Width / 2;
            var rightX = _pad.Center.X + _pad.Width / 2;
            var padY = _pad.Center.Y + _pad.Height / 2;
            return leftX < x && x < rightX && y < padY + 1e-5f;
        }

        public void IncreaseSpeed()
        {
            _pad.Velocity = _pad.Velocity * 1.25f;
        }

        public void DecreaseSpeed()
        {
            _pad.Velocity = _pad.Velocity / 1.25f;
        }

        public void IncreaseWidth()
        {
            _pad.Width = _pad.Width * 1.25f;
        }

        public void DecreaseWidth()
        {
            _pad.Width = _pad.Width / 1.25f;
        }

    }
}
