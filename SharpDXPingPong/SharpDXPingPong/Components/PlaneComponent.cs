using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Engine;

namespace SharpDXPingPong.Components
{
    internal class PlaneComponent : GameComponent
    {
        private readonly List<Vector4> _points = new List<Vector4>();
        private readonly Camera _camera;

        public PlaneComponent(
            Game game,
            string vertexShaderFilename,
            string pixelShaderFilename,
            Camera camera)
            : base(game, vertexShaderFilename, pixelShaderFilename)
        {
            _camera = camera;
        }

        protected override Vector4[] GetPoints()
        {
            const float step = 10.0f;
            const float dist = 1000.0f;

            for (var i = (int)(-dist / step); i < (int)(dist / step) + 1; i++)
            {
                _points.Add(new Vector4(step * i, dist, 0.0f, 1.0f)); _points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
                _points.Add(new Vector4(step * i, -dist, 0.0f, 1.0f)); _points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));

                _points.Add(new Vector4(dist, step * i, 0.0f, 1.0f)); _points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
                _points.Add(new Vector4(-dist, step * i, 0.0f, 1.0f)); _points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
            }

            _points.Add(Vector4.Zero); _points.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            _points.Add(Vector4.UnitX * 100.0f); _points.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));

            _points.Add(Vector4.Zero); _points.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
            _points.Add(Vector4.UnitY * 100.0f); _points.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

            _points.Add(Vector4.Zero); _points.Add(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
            _points.Add(Vector4.UnitZ * 100.0f); _points.Add(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

            return _points.ToArray();
        }

        protected override BufferDescription GetBufferDescription()
        {
            return new BufferDescription
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                SizeInBytes = _points.Count * Utilities.SizeOf<Vector4>() * 2,
                StructureByteStride = Utilities.SizeOf<Vector4>() * 2
            };
        }

        protected override PrimitiveTopology GetPrimitiveTopology()
        {
            return PrimitiveTopology.LineList;
        }

        protected override RasterizerState GetRasterizerState()
        {
            return new RasterizerState(Game.Device, new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid
            });
        }

        public override void Update(float time)
        {
            var worldViewProj = _camera.ViewMatrix * _camera.GetProjectionMatrix();
            Game.Context.UpdateSubresource(ref worldViewProj, StaticContantBuffer);
        }

        public override void Draw(float time)
        {
            var oldState = Game.Context.Rasterizer.State;
            Game.Context.Rasterizer.State = RasterizerState;
            SetContext();
            Game.Context.Draw(_points.Count, 0);
            Game.Context.Rasterizer.State = oldState;
        }
    }
}

