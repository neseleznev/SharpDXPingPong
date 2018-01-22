using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.IO;

namespace Engine
{
    public abstract class GameComponent : IGameComponent
    {
        protected readonly Game Game;
        protected VertexShader VertexShader;
        protected readonly string VertexShaderFilename;
        protected CompilationResult VertexShaderByteCode;
        public PixelShader PixelShader;
        protected readonly string PixelShaderFilename;
        protected CompilationResult PixelShaderByteCode;

        protected InputLayout Layout;

        protected BufferDescription BufferDescription;
        protected Buffer StaticContantBuffer;
        protected Buffer VertexBuffer;
        protected PrimitiveTopology PrimitiveTopology;
        protected VertexBufferBinding VertexBufferBinding;
        protected RasterizerState RasterizerState;

        protected GameComponent(
            Game game,
            string vertexShaderFilename,
            string pixelShaderFilename)
        {
            Game = game;
            VertexShaderFilename = vertexShaderFilename;
            PixelShaderFilename = pixelShaderFilename;
        }

        protected abstract Vector4[] GetPoints();

        protected abstract BufferDescription GetBufferDescription();

        protected abstract PrimitiveTopology GetPrimitiveTopology();

        protected abstract RasterizerState GetRasterizerState();

        protected void InitBuffer()
        {
            var points = GetPoints();
            BufferDescription = GetBufferDescription();
            var primitiveTopology = GetPrimitiveTopology();
            VertexBuffer = Buffer.Create(Game.Device, points, BufferDescription);
            PrimitiveTopology = primitiveTopology;
            VertexBufferBinding = new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<Vector4>() * 2, 0);
        }

        private void CompileShaders()
        {
            VertexShaderByteCode = ShaderBytecode.Compile(File.ReadAllText(VertexShaderFilename), "VSMain", "vs_5_0",
                ShaderFlags.PackMatrixRowMajor);
            VertexShader = new VertexShader(Game.Device, VertexShaderByteCode);

            PixelShaderByteCode = ShaderBytecode.Compile(File.ReadAllText(PixelShaderFilename), "PSMain", "ps_5_0",
                ShaderFlags.PackMatrixRowMajor);
            PixelShader = new PixelShader(Game.Device, PixelShaderByteCode);
        }

        protected virtual void InitInputLayout()
        {
            Layout = new InputLayout(
                Game.Device,
                ShaderSignature.GetInputSignature(VertexShaderByteCode),
                new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                }
            );
        }

        private void InitConstantBuffer()
        {
            // Create Constant Buffer
            StaticContantBuffer = new Buffer(Game.Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            // dynamicConstantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
        }

        protected virtual void SetContext()
        {
            Game.Context.InputAssembler.InputLayout = Layout;
            Game.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology;
            Game.Context.InputAssembler.SetVertexBuffers(0, VertexBufferBinding);
            if (StaticContantBuffer != null)
            {
                Game.Context.VertexShader.SetConstantBuffer(0, StaticContantBuffer);
            }

            Game.Context.VertexShader.Set(VertexShader);
            Game.Context.PixelShader.Set(PixelShader);
        }

        public void Init()
        {
            InitBuffer();
            CompileShaders();
            InitInputLayout();
            InitConstantBuffer();
            RasterizerState = GetRasterizerState();
        }

        public abstract void Update(float deltaTime);

        public abstract void Draw(float deltaTime);

        public void Dispose()
        {
            VertexShaderByteCode.Dispose();
            VertexShader.Dispose();
            PixelShaderByteCode.Dispose();
            PixelShader.Dispose();
            Layout.Dispose();
            VertexBuffer.Dispose();
        }
    }
}