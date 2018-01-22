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

        float height = 0.1f;
        float width = 0.3f;
        Vector2 center;
        private float sensitivity = 0.01f;

        public PadComponent(
            Game game,
            string vertexShaderFilename,
            string pixelShaderFilename,
            Camera camera)
            : base(game, vertexShaderFilename, pixelShaderFilename)
        {
            this._camera = camera;
            center = new Vector2(0.0f, -1 + 0.00625f + this.height);
        }

        protected override Vector4[] GetPoints()
        {
            _points = new List<Vector4>()
            {
                new Vector4(center.X - width / 2, center.Y + height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(center.X + width / 2, center.Y + height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(center.X + width / 2, center.Y - height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(center.X - width / 2, center.Y + height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(center.X - width / 2, center.Y - height / 2, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(center.X + width / 2, center.Y - height / 2, 0.0f, 1.0f),
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

//        protected float HorizontalPercentToWorld(float percent)
//        {
//            float horizontalPixels = Game.GetWidth() / 100.0f * percent;
//            _camera.ScreenToWorld(new Vector4(horizontalPixels))
//        }

        public override void Update(float deltaTime)
        {
            var worldViewProj = _camera.ViewMatrix * _camera.GetProjectionMatrix();

            if (Game.InputDevice.IsKeyDown(Keys.A))
                center.X += 1 * deltaTime;
            if (Game.InputDevice.IsKeyDown(Keys.D))
                center.X -= 1 * deltaTime;
            center.X = Math.Min(1 - width / 2, Math.Max(-1 + width / 2, center.X));

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