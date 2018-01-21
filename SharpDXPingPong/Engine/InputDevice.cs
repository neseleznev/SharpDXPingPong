using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.RawInput;

namespace Engine
{
    public class InputDevice
    {
        private readonly Game _game;

        public HashSet<Keys> PressedKeys = new HashSet<Keys>();

        public Vector2 MousePositionLocal { get; private set; }
        public Vector2 MouseOffset { get; private set; }

        public struct MouseMoveEventArgs
        {
            public Vector2 Position;
            public Vector2 Offset;
        }

        public event Action<MouseMoveEventArgs> MouseMove;

        internal InputDevice(Game game)
        {
            _game = game;

            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
            Device.MouseInput += Device_MouseInput;

            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
            Device.KeyboardInput += Device_KeyboardInput;
        }

        private void Device_KeyboardInput(object sender, KeyboardInputEventArgs e)
        {
            var Break = e.ScanCodeFlags.HasFlag(ScanCodeFlags.Break);

            if (Break)
            {
                if (PressedKeys.Contains(e.Key)) RemovePressedKey(e.Key);
            }
            else
            {
                if (!PressedKeys.Contains(e.Key)) AddPressedKey(e.Key);
            }
        }

        private void Device_MouseInput(object sender, MouseInputEventArgs e)
        {
            var p = _game.Form.PointToClient(Cursor.Position);

            MousePositionLocal = new Vector2(p.X, p.Y);
            MouseOffset = new Vector2(e.X, e.Y);

            MouseMove?.Invoke(new MouseMoveEventArgs() { Position = MousePositionLocal, Offset = MouseOffset });
        }

        /// <summary>
        /// Adds key to hash list and fires KeyDown event
        /// </summary>
        /// <param name="key"></param>
        void AddPressedKey(Keys key)
        {
            //            if (!_game.IsActive)
            //            {
            //                return;
            //            }

            PressedKeys.Add(key);
            //if (KeyDown != null)
            //{
            //	KeyDown(this, new KeyEventArgs() { Key = key });
            //}
        }

        /// <summary>
        /// Removes key from hash list and fires KeyUp event
        /// </summary>
        /// <param name="key"></param>
        void RemovePressedKey(Keys key)
        {
            if (PressedKeys.Contains(key))
            {
                PressedKeys.Remove(key);
                //if (KeyUp != null)
                //{
                //	KeyUp(this, new KeyEventArgs() { Key = key });
                //}
            }
        }

        public bool IsKeyDown(Keys key, bool ignoreInputMode = true)
        {
            return (PressedKeys.Contains(key));
        }

        public bool IsKeyUp(Keys key)
        {
            return !IsKeyDown(key);
        }
    }
}
