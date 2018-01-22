using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        internal Vector2 Center;
        internal float Velocity { get; set; }

        public PadComponent(
            Game game,
            string vertexShaderFilename,
            string pixelShaderFilename,
            Camera camera)
            : base(game, vertexShaderFilename, pixelShaderFilename)
        {
            this._camera = camera;
            Reset();
        }

        public void Reset()
        {
            Height = 0.1f;
            Width = 0.25f;
            Center = new Vector2(0.0f, -1 + 0.00625f + this.Height);
            Velocity = 1.0f;
        }
        protected override Vector4[] GetPoints()
        {
            _points = new List<Vector4>()
            {
                new Vector4(Center.X - Width / 2, Center.Y + Height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(Center.X + Width / 2, Center.Y + Height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(Center.X + Width / 2, Center.Y - Height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(Center.X - Width / 2, Center.Y + Height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(Center.X - Width / 2, Center.Y - Height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(Center.X + Width / 2, Center.Y - Height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f)
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
            var worldViewProj = _camera.ViewMatrix * _camera.GetProjectionMatrix();
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
    }
}