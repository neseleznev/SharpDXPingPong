using System;
using System.IO;
using Engine;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SharpDXPingPong.Components
{
    public class TexturedQuadComponent : IGameComponent
    {
        PixelShader pixelShader;
        VertexShader vertexShader;
        CompilationResult pixelShaderByteCode;
        CompilationResult vertexShaderByteCode;
        InputLayout layout;
        Vector4[] points;
        Buffer vertices;
        VertexBufferBinding bufBinding;
        Camera camera;
        Buffer constantBuffer;


        Texture2D texture;
        ShaderResourceView texSRV;
        SamplerState sampler;

        string textureName = "";

        RasterizerState rastState;

        public Vector3 Position;
        private Vector3 _center;
        private readonly float _size;
        private Game Game;

        public TexturedQuadComponent(Game game, string textureName, Camera cam, Vector3 center, float size)
        {
            Game = game;
            camera = cam;

            this.textureName = textureName;

            Position = new Vector3(0, 0, 0);
            _center = center;
            this._size = size;
        }


        public void Init()
        {
            // Compile Vertex and Pixel shaders
            vertexShaderByteCode = ShaderBytecode.CompileFromFile("TexturedShader.hlsl", "VSMain", "vs_5_0", ShaderFlags.PackMatrixRowMajor);

            if (vertexShaderByteCode.Message != null)
            {
                Console.WriteLine(vertexShaderByteCode.Message);
            }


            vertexShader = new VertexShader(Game.Device, vertexShaderByteCode);

            pixelShaderByteCode = ShaderBytecode.CompileFromFile("TexturedShader.hlsl", "PSMain", "ps_5_0", ShaderFlags.PackMatrixRowMajor);
            pixelShader = new PixelShader(Game.Device, pixelShaderByteCode);


            // Layout from VertexShader input signature
            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            layout = new InputLayout(
                Game.Device,
                signature,
                new[] {
                        new InputElement("POSITION",    0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("TEXCOORD",    0, Format.R32G32B32A32_Float, 16, 0)
                    });
            
            points = new[] {
                new Vector4(_center.X + _size, _center.Y + _size, _center.Z, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(_center.X - _size, _center.Y + _size, _center.Z, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(_center.X + _size, _center.Y - _size, _center.Z, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(_center.X - _size, _center.Y - _size, _center.Z, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 0.0f),
            };

            var bufDesc = new BufferDescription
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                SizeInBytes = sizeof(float) * 4 * 8,
                StructureByteStride = 32
            };

            vertices = Buffer.Create(Game.Device, points, bufDesc);

            bufBinding = new VertexBufferBinding(vertices, 32, 0);

            constantBuffer = new Buffer(Game.Device, new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utilities.SizeOf<Matrix>(),
                Usage = ResourceUsage.Default
            });


            rastState = new RasterizerState(Game.Device, new RasterizerStateDescription
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid
            });


            if (File.Exists(textureName))
            {
                texture = Game.TextureLoader.LoadTextureFromFile(textureName);
                texSRV = new ShaderResourceView(Game.Device, texture);
            }
            sampler = new SamplerState(Game.Device, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipLinear,
                ComparisonFunction = Comparison.Always,
                BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 0.0f, 0.0f, 1.0f),
                MaximumLod = int.MaxValue
            });

        }


        public void Update(float deltaTime)
        {
            var world = Matrix.Translation(Position);
            var proj = world * camera.ViewMatrix * camera.GetProjectionMatrix();

            Game.Context.UpdateSubresource(ref proj, constantBuffer);
        }


        public void Draw(float deltaTime)
        {
            var context = Game.Context;

            var oldState = context.Rasterizer.State;
            context.Rasterizer.State = rastState;

            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            context.InputAssembler.SetVertexBuffers(0, bufBinding);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            context.VertexShader.SetConstantBuffer(0, constantBuffer);
            context.PixelShader.SetShaderResource(0, texSRV);
            context.PixelShader.SetSampler(0, sampler);

            PixHelper.BeginEvent(Color.Red, "Textured Triangle Draw Event");
            context.Draw(4, 0);
            PixHelper.EndEvent();

            context.Rasterizer.State = oldState;
        }

        public void Dispose()
        {
            pixelShader.Dispose();
            vertexShader.Dispose();
            pixelShaderByteCode.Dispose();
            vertexShaderByteCode.Dispose();
            layout.Dispose();
            vertices.Dispose();

            rastState.Dispose();

            constantBuffer.Dispose();
        }
    }
}
