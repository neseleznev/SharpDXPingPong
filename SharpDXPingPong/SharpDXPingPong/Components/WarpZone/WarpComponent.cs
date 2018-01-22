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
        
        internal float Radius;
        internal Vector3 Position;
        internal float Velocity { get; set; }
        private readonly Random _rnd;
        private bool _appeared;
        
        public WarpComponent(Game game, string vertexShaderFilename, string pixelShaderFilename, Camera camera)
            : base(game, vertexShaderFilename, pixelShaderFilename)
        {
            _camera = camera;
            Radius = 15f;
            Position = new Vector3(Game.GetWidth() / 2, Game.GetHeight() / 2, 0);
            Velocity = 50f;
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
                var x = (float)(Radius * Math.Cos(theta));
                var y = (float)(-Radius * Math.Sin(theta));
                _points.Add(new Vector4(x, y, 0.0f, 1.0f));
                _points.Add(GetColor());

                var theta1 = (i + 1) * wedgeAngle;
                var x1 = (float)(Radius * Math.Cos(theta1));
                var y1 = (float)(-Radius * Math.Sin(theta1));
                _points.Add(new Vector4(x1, y1, 0.0f, 1.0f));
                _points.Add(GetColor());

                _points.Add(new Vector4(0, 0, 0, 1.0f));
                _points.Add(GetColor());
            }

            _points.Add(new Vector4(0, 0, 1, 1.0f));
            _points.Add(GetColor());

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
            var worldViewProj = Matrix.Translation(Position) * _camera.GetProjectionMatrixOrhographic();
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
            Position.X = (float) (_rnd.NextDouble() * Game.GetWidth() * 0.9f);
            Position.Y = Game.GetHeight() / 2;
            _appeared = true;
        }

        public void Hide()
        {
            Position.X = - Game.GetWidth();
            Position.Y = - Game.GetHeight();
            _appeared = false;
        }
    }
}
