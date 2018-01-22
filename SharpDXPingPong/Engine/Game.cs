using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Color = SharpDX.Color;
using Device = SharpDX.Direct3D11.Device;

namespace Engine
{
    public class Game : ISizeable
    {
        public static Game Instance { protected set; get; }
        public string Name { protected set; get; }
        private int _windowWidth, _windowHeight;

        protected readonly List<IGameComponent> Components;

        public RenderForm Form;
        public InputDevice InputDevice;
        public Device Device;
        protected SwapChain SwapChain;
        protected SwapChainDescription SwapDesc;
        protected Texture2D BackBuffer;
        protected Texture2D DepthBuffer;
        protected DepthStencilView DepthView;
        protected RenderTargetView RenderView;
        public DeviceContext Context;
        private Stopwatch _clock;
        public TimeSpan TotalTime { protected set; get; }
        private bool _isShutdownRequested;

        protected Game(string name, int width = 800, int height = 600)
        {
            Name = name;
            _windowWidth = width;
            _windowHeight = height;

            Components = new List<IGameComponent>();

            InputDevice = new InputDevice(this);

            InitResources();
            InitBackBuffer();
            InitContext();
            SetupViewport();

            //	For animation rendering applications :
            //	http://msdn.microsoft.com/en-us/library/bb384202.aspx
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            // Singleton Pattern
            Instance = this;
        }

        private void InitResources()
        {
            Form = new RenderForm(Name)
            {
                ClientSize = new System.Drawing.Size(_windowWidth, _windowHeight)
            };

            SwapDesc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(
                    Form.ClientSize.Width, Form.ClientSize.Height,
                    new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = Form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(
                DriverType.Hardware,
#if DEBUG
                DeviceCreationFlags.Debug,
#else
				DeviceCreationFlags.None,
#endif
                SwapDesc,
                out Device,
                out SwapChain
            );

            // Ignore all windows events
            var factory = SwapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(Form.Handle, WindowAssociationFlags.IgnoreAll);
        }

        private void InitBackBuffer()
        {
            // Get the backbuffer from the swapchain
            BackBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(SwapChain, 0);

            // Renderview on the backbuffer
            RenderView = new RenderTargetView(Device, BackBuffer);

            // Create the depth buffer
            DepthBuffer = new Texture2D(Device, new Texture2DDescription
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = Form.ClientSize.Width,
                Height = Form.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            // Create the depth buffer view
            DepthView = new DepthStencilView(Device, DepthBuffer
//                , new DepthStencilViewDescription
//            {
//                Format = Format.D32_Float,
//                Dimension = DepthStencilViewDimension.Texture2D,
//                Flags = DepthStencilViewFlags.None
//            }
            );
        }

        private void InitContext()
        {
            Context = Device.ImmediateContext;
        }

        private void SetupViewport()
        {
            // Setup targets and viewport for rendering
            Context.Rasterizer.SetViewport(new Viewport(0, 0, _windowWidth, _windowHeight, 0.0f, 1.0f));
            // SetBackBufferForOutputMergerer
            Context.OutputMerger.SetTargets(RenderView);
        }
        
        private void OnWindowResize(object sender, EventArgs args)
        {
            _windowWidth = Form.ClientSize.Width;
            _windowHeight = Form.ClientSize.Height;

            // Dispose all previous allocated resources
            Utilities.Dispose(ref BackBuffer);
            Utilities.Dispose(ref RenderView);
            Utilities.Dispose(ref DepthBuffer);
            Utilities.Dispose(ref DepthView);

            // Resize the backbuffer
            SwapChain.ResizeBuffers(SwapDesc.BufferCount, _windowWidth, _windowHeight,
                Format.Unknown, SwapChainFlags.None);

            InitBackBuffer();
            SetupViewport();
        }

        public void Run()
        {
            Init();
            Components.ForEach(component => component.Init());

            Form.UserResized += OnWindowResize;
            OnWindowResize(null, null);

            _clock = new Stopwatch();
            _clock.Start();
            TotalTime = _clock.Elapsed;
            
            RenderLoop.Run(Form, () =>
            {
                ClearFrame();
                var curTime = _clock.Elapsed;
                var deltaTime = (float)(curTime - TotalTime).TotalSeconds;
                TotalTime = curTime;

                Update(deltaTime);
                Draw(deltaTime);
                PresentFrame();

                if (_isShutdownRequested)
                    Form.Close();
            });

            _clock.Stop();
            Dispose();
        }

        // 
        protected virtual void Init()
        {

        }

        protected void ClearFrame()
        {
            // Clear views
            Context.ClearState();

            Context.OutputMerger.SetTargets(DepthView, RenderView);
            Context.Rasterizer.SetViewport(new Viewport(0, 0, _windowWidth, _windowHeight, 0.0f, 1.0f));

            Context.ClearRenderTargetView(RenderView, Color.Black);
            Context.ClearDepthStencilView(DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        protected virtual void Update(float deltaTime)
        {
            Components.ForEach(x => x.Update(deltaTime));
        }

        protected virtual void Draw(float deltaTime)
        {
            Components.ForEach(x => x.Draw(deltaTime));
        }

        protected void PresentFrame()
        {
            SwapChain.Present(1, PresentFlags.None);
        }

        public void Dispose()
        {
            Context.ClearState();
            Context.Flush();
            foreach (var t in Components)
            {
                t.Dispose();
            }

            RenderView.Dispose();
            BackBuffer.Dispose();

            Device.Dispose();
            Context.Dispose();
            SwapChain.Dispose();
            //factory.Dispose();
        }

        public void Shutdown()
        {
            _isShutdownRequested = true;
        }

        public float GetWidth()
        {
            return _windowWidth;
        }

        public float GetHeight()
        {
            return _windowHeight;
        }
    }
}