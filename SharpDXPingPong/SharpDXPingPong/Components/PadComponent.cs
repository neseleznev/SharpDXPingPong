using System.Collections.Generic;
using Engine;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace SharpDXPingPong.Components
{
    class PadComponent : GameComponent
    {
        private List<Vector4> _points = new List<Vector4>();
        private readonly Camera _camera;
        
        internal float Height;
        internal float Width { get; set; }
        internal Vector3 Position;
        internal float Velocity { get; set; }

        public PadComponent(
            Game game,
            string vertexShaderFilename,
            string pixelShaderFilename,
            Camera camera)
            : base(game, vertexShaderFilename, pixelShaderFilename)
        {
            _camera = camera;
            Reset();
        }

        public void Reset()
        {
            Height = 0.05f * Game.GetHeight();
            Width = 0.125f * Game.GetWidth();
            Position = new Vector3(Game.GetWidth()/2, Height * 0.125f, 0);
            Velocity = 1.0f * 300;
        }
        protected override Vector4[] GetPoints()
        {
            _points = new List<Vector4>()
            {
                new Vector4(0, Height, 0, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(Width, Height, 0, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(Width, 0, 0, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0, Height, 0, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0, 0, 0, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(Width, 0, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.0f)
            };
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
    }
}