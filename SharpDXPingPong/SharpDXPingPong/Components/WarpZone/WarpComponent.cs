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
    abstract class WarpComponent : GameComponent
    {

        private List<Vector4> _points = new List<Vector4>();
        private readonly Camera _camera;

        internal float BasicRadius { get; set; }
        internal Vector2 Radius;
        internal Vector2 Center;
        internal float Velocity { get; set; }
        internal Vector4 Color = new Vector4(0, 0, 0, 0);
        private readonly Random _rnd;
        private bool _appeared;
        
        public WarpComponent(Game game, string vertexShaderFilename, string pixelShaderFilename, Camera camera)
            : base(game, vertexShaderFilename, pixelShaderFilename)
        {
            _camera = camera;
            BasicRadius = 0.05f;
            Radius = new Vector2(BasicRadius, BasicRadius * Game.GetWidth() / Game.GetHeight());
            Center = new Vector2(10, 10);
            Velocity = 0.2f;
            _rnd = new Random();
        }

        protected abstract Vector4 GetColor();

        protected override Vector4[] GetPoints()
        {
            const int numpoints = 24;
            const float pi = 3.14159f;
            const float wedgeAngle = 2 * pi / numpoints;

            _points = new List<Vector4>();

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
                var x1 = (float)(Center.X + Radius.X * Math.Cos(theta1));
                var y1 = (float)(Center.Y - Radius.Y * Math.Sin(theta1));
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
            Center.Y = Center.Y - deltaTime * Velocity;
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

        public void DropRandomly()
        {
            if (_appeared)
                return;
            Center.X = (float) (_rnd.NextDouble() * 1.8 - 0.9);
            Center.Y = 0.0f;
            Color = GetColor();
            _appeared = true;
        }

        public void Hide()
        {
            Center.X = -2;
            Center.Y = -2;
            Color = new Vector4(0, 0, 0, 0);
            _appeared = false;
        }
    }
}
