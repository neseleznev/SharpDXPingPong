using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace SharpDXPingPong.Components
{
    class BallComponent : GameComponent
    {
        private List<Vector4> _points = new List<Vector4>();
        private readonly Camera _camera;

        internal float BasicRadius { get; set; }
        internal Vector2 Radius;
        internal Vector2 Center;
        internal float Velocity { get; set; }
        
        internal float verticalDirection;
        internal float horizontalDirection;

        public BallComponent(Game game, string vertexShaderFilename, string pixelShaderFilename, Camera camera)
            : base(game, vertexShaderFilename, pixelShaderFilename)
        {
            _camera = camera;
            Reset();
        }

        public void Reset()
        {
            verticalDirection = -1;  // down
            horizontalDirection = -1;  // left

            BasicRadius = 0.05f;
            Radius = new Vector2(BasicRadius, BasicRadius * Game.GetWidth() / Game.GetHeight());
            Center = new Vector2(0, 0);
            Velocity = 0.007f;
        }
        protected override Vector4[] GetPoints()
        {
            const int numpoints = 24;
            const float pi = 3.14159f;
            const float wedgeAngle = 2 * pi / numpoints;

            _points = new List<Vector4>();
            Vector4 Color = new Vector4(212f, 175f, 55f, 1.0f);

            for (var i = 0; i < numpoints; i++)
            {
                //Calculate theta for this vertex
                var theta = i * wedgeAngle;

                //Compute X and Y locations
                var x = (float)(Center.X + Radius.X * Math.Cos(theta));
                var y = (float)(Center.Y - Radius.Y * Math.Sin(theta));
                _points.Add(new Vector4(x, y, 0.0f, 1.0f));
                _points.Add(Color);

                var theta1 = (i + 1) * wedgeAngle;
                var x1 = (float) (Center.X + Radius.X * Math.Cos(theta1));
                var y1 = (float) (Center.Y - Radius.Y * Math.Sin(theta1));
                _points.Add(new Vector4(x1, y1, 0.0f, 1.0f));
                _points.Add(Color);

                _points.Add(new Vector4(Center.X, Center.Y, 0.0f, 1.0f));
                _points.Add(Color);
            }

            _points.Add(new Vector4(Center.X, Center.Y, 1.0f, 1.0f));
            _points.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

            return _points.ToArray();
        }

        protected override BufferDescription GetBufferDescription()
        {
            return new BufferDescription
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default
            };
        }

        protected override PrimitiveTopology GetPrimitiveTopology()
        {
            return PrimitiveTopology.TriangleList;
        }

        protected override RasterizerState GetRasterizerState()
        {
            return new RasterizerState(Game.Device, new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid
            });
        }

        public override void Update(float deltaTime)
        {
            var worldViewProj = _camera.ViewMatrix * _camera.GetProjectionMatrix();

            Radius.X = BasicRadius;
            Radius.Y = BasicRadius * Game.GetWidth() / Game.GetHeight();
            InitBuffer();
            Game.Context.UpdateSubresource(ref worldViewProj, StaticContantBuffer);
        }

        public override void Draw(float deltaTime)
        {
            var oldState = Game.Context.Rasterizer.State;
            Game.Context.Rasterizer.State = RasterizerState;
            SetContext();
            Game.Context.Draw(_points.Count, 0);
            Game.Context.Rasterizer.State = oldState;
        }

        public void IncreaseSpeed()
        {
            Velocity *= 1.25f;
        }

        public void DecreaseSpeed()
        {
            Velocity /= 1.25f;
        }
    }
}
