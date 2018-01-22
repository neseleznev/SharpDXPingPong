using System;
using System.Collections.Generic;
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
        private readonly Random _random;
        
        internal float Radius;
        internal Vector3 Position;
        internal float Velocity { get; set; }
        
        internal float VerticalDirection;
        internal float HorizontalDirection;

        public BallComponent(Game game, string vertexShaderFilename, string pixelShaderFilename, Camera camera)
            : base(game, vertexShaderFilename, pixelShaderFilename)
        {
            _camera = camera;
            _random = new Random();
            Reset();
        }

        public void Reset()
        {
            VerticalDirection = -1;  // down
            HorizontalDirection = (float) (_random.NextDouble() * 5 - 2.5f);  // left
            
            Radius = 20f;
            Position = new Vector3(Game.GetWidth() / 2, Game.GetHeight() * 0.75f, 0);
            Velocity = 1f;
        }

        protected override Vector4[] GetPoints()
        {
            const int numpoints = 24;
            const float pi = 3.14159f;
            const float wedgeAngle = 2 * pi / numpoints;

            _points = new List<Vector4>();
            var color = new Vector4(220f / 256f, 175f / 256f, 55f / 256f, 1.0f);

            for (var i = 0; i < numpoints; i++)
            {
                //Calculate theta for this vertex
                var theta = i * wedgeAngle;

                //Compute X and Y locations
                var x = (float)(Radius * Math.Cos(theta));
                var y = (float)(-Radius * Math.Sin(theta));
                _points.Add(new Vector4(x, y, 0.0f, 1.0f));
                _points.Add(color);

                var theta1 = (i + 1) * wedgeAngle;
                var x1 = (float) (Radius * Math.Cos(theta1));
                var y1 = (float) (-Radius * Math.Sin(theta1));
                _points.Add(new Vector4(x1, y1, 0.0f, 1.0f));
                _points.Add(color);

                _points.Add(new Vector4(0, 0, 0, 1.0f));
                _points.Add(color);
            }
            
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
