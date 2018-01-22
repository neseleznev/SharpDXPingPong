using SharpDX;

namespace Engine
{
    public class Camera
    {
        private readonly ISizeable _sizeable;

        public Matrix ViewMatrix { internal set; get; }

        public Camera(ISizeable sizeable)
        {
            _sizeable = sizeable;
            ViewMatrix = Matrix.Identity;
        }

        public Matrix GetProjectionMatrix()
        {
            //return Matrix.OrthoOffCenterLH(0, Game.Form.ClientSize.Width, 0, Game.Form.ClientSize.Height, 0.1f, 1000.0f);
            //return Matrix.OrthoLH(Game.Form.ClientSize.Width, Game.Form.ClientSize.Height, 0.1f, 1000.0f);
            return Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(45), _sizeable.GetWidth() / _sizeable.GetHeight(),
                0.1f, 10000.0f);
            //return ProjMatrix;
        }

        public Matrix GetProjectionMatrixOrhographic()
        {
            return Matrix.OrthoOffCenterLH(0, _sizeable.GetWidth(), 0, _sizeable.GetHeight(), 0.1f, 1000.0f);
        }

        public Vector2 WorldToScreen(Vector4 target)
        {
            var worldViewProj = ViewMatrix * GetProjectionMatrix();
            var projectedVector = Vector4.Transform(target, worldViewProj);

            var screenX = (projectedVector.X / projectedVector.W + 1.0f) / 2.0f * _sizeable.GetWidth();
            var screenY = (1.0f - projectedVector.Y / projectedVector.W) / 2.0f * _sizeable.GetHeight();
            return new Vector2(screenX, screenY);
        }

        public Vector4 ScreenToWorld(Vector2 screen)
        {
            var worldViewProj = ViewMatrix * GetProjectionMatrix();
            Matrix worldInv = new Matrix(worldViewProj.ToArray());
            worldInv.Invert();
            Vector4 target = Vector4.Transform(
                new Vector4(screen.X / _sizeable.GetWidth() * 2 - 1.0f,
                    screen.Y / _sizeable.GetHeight() * 2 - 1.0f,
                    1.0f, 1.0f), worldInv);
            target.Normalize();
            return target;
        }

//        private static Camera _instance;
//        private Camera() { }
//        public static Camera Instance => _instance ?? (_instance = new Camera());
//
//        public Vector3 Position = new Vector3(0, 500, 1);
//
//        public Matrix ViewMatrix = Matrix.LookAtLH(new Vector3(0, 500, 1), new Vector3(0, 0, 0), Vector3.UnitY);
//        public Matrix ProjMatrix = Matrix.Identity;
//        private float _aspect;
//
//        private float _yaw = 0;
//        public float _pitch = 0;
//
//        public static void Shift(float deltaX, float deltaY)
//        {
//            Instance._yaw += deltaX * 0.003f;
//            Instance._pitch += deltaY * 0.003f;
//
//            var rotation = Matrix.RotationYawPitchRoll(Instance._yaw, Instance._pitch, 0);
//
//            //            var newSource = new Vector3(
//            //                Instance.Position.X + deltaX / multiplier,
//            //                Instance.Position.Y - deltaY / multiplier,
//            //                Instance.Position.Z
//            //            );
//            //            var newDestination = new Vector3(
//            //                Instance.oldDestination.X,
//            //                Instance.oldDestination.Y,
//            //                Instance.oldDestination.Z
//            //            );
//            //            Console.WriteLine(newDestination);
//            //            Instance._viewMatrix = Matrix.LookAtLH(newSource, newDestination, Vector3.UnitY);
//            //            Instance.Position = newSource;
//            //            Instance.oldDestination = newDestination;
//            Instance.ViewMatrix =
//                Matrix.LookAtLH(Instance.Position, Instance.Position + rotation.Backward, rotation.Up);
//        }
//
//        public static void SetAspectRatio(float aspect)
//        {
//            Instance._aspect = aspect;
//            //return Matrix.OrthoOffCenterLH(0, Game.Form.ClientSize.Width, 0, Game.Form.ClientSize.Height, 0.1f, 1000.0f);
//            //return Matrix.OrthoLH(Game.Form.ClientSize.Width, Game.Form.ClientSize.Height, 0.1f, 1000.0f);
//            Instance.ProjMatrix = Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(90), Instance._aspect, 0.1f, 1000.0f);
//        }
    }
}