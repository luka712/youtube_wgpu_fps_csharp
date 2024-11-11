using System.Runtime.InteropServices;
using FPSGame.Extensions;
using FPSGame.Input;
using FPSGame.Utils;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;

using WGPUTexture = Silk.NET.WebGPU.Texture;

namespace FPSGame
{
    public unsafe class Engine : IDisposable
    {
        private Instance* instance;
        private Surface* surface;
        private Adapter* adapter;

        private SurfaceTexture surfaceTexture;
        private TextureView* surfaceTextureView;

        private WGPUTexture* depthTexture;
        private TextureView* depthTextureView;

        public event Action OnInitialize;
        public event Action OnUpdate;
        public event Action OnRender;
        public event Action OnDispose;
        
        public IWindow Window { get; private set; }
        
        public InputManager Input { get; private set; }

        public WebGPU WGPU {get; private set; }

        public Device* Device { get; private set; }

        public Queue* Queue { get; private set; }

        public CommandEncoder* CurrentCommandEncoder { get; private set; }

        public TextureFormat PreferredTextureFormat => TextureFormat.Bgra8Unorm;

        public RenderPassEncoder* CurrentRenderPassEncoder { get; private set; }

        public void Initialize()
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Size = new Vector2D<int>(1280, 720);
            windowOptions.Title = "FPS Game";
            windowOptions.API = GraphicsAPI.None;

            Window = Silk.NET.Windowing.Window.Create(windowOptions);
            Window.Initialize();
            
            Input = new InputManager(Window);

            // Setup WGPU.
            CreateApi();
            CreateInstance();
            CreateSurface();
            CreateAdapter();
            CreateDevice();
            ConfigureSurface();
            ConfigureDebugCallback();
            ConfigureDepthStencilTexture();

            Window.Load += Window_OnLoad;
            Window.Update += Window_OnUpdate;
            Window.Render += Window_OnRender;

            // - QUEUE
            Queue = WGPU.DeviceGetQueue(Device);

            OnInitialize?.Invoke();

            Window.Run();
        }

        private void CreateApi()
        {
            WGPU = WebGPU.GetApi();
            Console.WriteLine("Created WGPU API.");
        }

        private void CreateInstance()
        {
            InstanceDescriptor descriptor = new InstanceDescriptor();
            instance = WGPU.CreateInstance(descriptor);
            Console.WriteLine("Created WGPU Instance.");
        }

        private void CreateSurface()
        {
            surface = Window.CreateWebGPUSurface(WGPU, instance);
            Console.WriteLine("Created WGPU Surface.");
        }

        private void CreateAdapter()
        {
            RequestAdapterOptions options = new RequestAdapterOptions();
            options.CompatibleSurface = surface;
            options.BackendType = BackendType.Vulkan;
            options.PowerPreference = PowerPreference.HighPerformance;

            PfnRequestAdapterCallback callback = PfnRequestAdapterCallback.From(
                (status, wgpuAdapter, msgPtr, userDataPtr) =>
                {
                    if (status == RequestAdapterStatus.Success)
                    {
                        this.adapter = wgpuAdapter;
                        Console.WriteLine("Retrieved WGPU Adapter.");
                    }
                    else
                    {
                        string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                        Console.WriteLine($"Error while retrieving WGPU Adapter: {msg}");
                    }
                });

            WGPU.InstanceRequestAdapter(instance, options, callback, null);
        }

        private void CreateDevice()
        {
            PfnRequestDeviceCallback callback = PfnRequestDeviceCallback.From(
                (status, wgpuDevice, msgPtr, userDataPtr) =>
                {
                    if (status == RequestDeviceStatus.Success)
                    {
                        this.Device = wgpuDevice;
                        Console.WriteLine("Retrieved WGPU Device.");
                    }
                    else
                    {
                        string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                        Console.WriteLine($"Error while retrieving WGPU Device: {msg}");
                    }
                });

            DeviceDescriptor descriptor = new DeviceDescriptor();
            WGPU.AdapterRequestDevice(adapter, descriptor, callback, null);
        }

        private void ConfigureSurface()
        {
            SurfaceConfiguration configuration = new SurfaceConfiguration();
            configuration.Device = Device;
            configuration.Width = (uint)Window.Size.X;
            configuration.Height = (uint)Window.Size.Y;
            configuration.Format = PreferredTextureFormat;
            configuration.PresentMode = PresentMode.Fifo;
            configuration.Usage = TextureUsage.RenderAttachment;

            WGPU.SurfaceConfigure(surface, configuration);
        }

        private void ConfigureDebugCallback()
        {
            PfnErrorCallback callback = PfnErrorCallback.From((type, msgPtr, userDataPtr) =>
            {
                string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                Console.WriteLine($"WGPU Unhandled error callback: {msg}");
            });

            WGPU.DeviceSetUncapturedErrorCallback(Device, callback, null);
            Console.WriteLine("WGPU Debug Callback Configured.");
        }

        private void ConfigureDepthStencilTexture()
        {
            // Dispose of previous depth texture and view. Might need to be done if resizing the window.
            if (depthTextureView != null)
            {
                WGPU.TextureViewRelease(depthTextureView);
            }

            if (depthTexture != null)
            {
                WGPU.TextureRelease(depthTexture);
            }

            depthTexture = WebGPUUtil.Texture.CreateDepthTexture(this, (uint)Window.Size.X, (uint)Window.Size.Y);
            depthTextureView = WebGPUUtil.TextureView.Create(this, depthTexture, 
                format: TextureFormat.Depth24PlusStencil8,
                label: "Depth Texture View");
        }

        private void Window_OnLoad()
        {
        }

        private void Window_OnUpdate(double dt)
        {
            OnUpdate?.Invoke();
            
            Input.AfterUpdate();
        }

        private void Window_OnRender(double dt)
        {
            BeforeRender();

            OnRender?.Invoke();

            AfterRender();
        }

        private void BeforeRender()
        {
            // - QUEUE
            Queue = WGPU.DeviceGetQueue(Device);

            // - COMMAND ENCODER
            CurrentCommandEncoder = WGPU.DeviceCreateCommandEncoder(Device, null);
            WGPU.CommandEncoderPushDebugGroup(CurrentCommandEncoder, "Main Render Pass".ToBytePtr());

            // - SURFACE TEXTURE
            WGPU.SurfaceGetCurrentTexture(surface, ref surfaceTexture);
            surfaceTextureView = WGPU.TextureCreateView(surfaceTexture.Texture, null);

            // - RENDER PASS ENCODER

            // -- COLOR ATTACHMENT
            RenderPassColorAttachment* colorAttachments = stackalloc RenderPassColorAttachment[1];
            colorAttachments[0].View = surfaceTextureView;
            colorAttachments[0].LoadOp = LoadOp.Clear;
            colorAttachments[0].ClearValue = new Color(0.1, 0.9, 0.9, 1.0);
            colorAttachments[0].StoreOp = StoreOp.Store;

            // -- DEPTH ATTACHMENT
            RenderPassDepthStencilAttachment depthStencilAttachment = new()
            {
                DepthClearValue = 1.0f,
                DepthLoadOp = LoadOp.Clear,
                DepthStoreOp = StoreOp.Store,
                StencilClearValue = 0,
                StencilLoadOp = LoadOp.Clear,
                StencilStoreOp = StoreOp.Store,
                View = depthTextureView
            };

            RenderPassDescriptor renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.ColorAttachments = colorAttachments;
            renderPassDescriptor.ColorAttachmentCount = 1;
            renderPassDescriptor.DepthStencilAttachment = &depthStencilAttachment;      

            CurrentRenderPassEncoder = WGPU.CommandEncoderBeginRenderPass(CurrentCommandEncoder, renderPassDescriptor);
        }

        private void AfterRender()
        {
            // - END RENDER PASS
            WGPU.CommandEncoderPopDebugGroup(CurrentCommandEncoder);
            WGPU.RenderPassEncoderEnd(CurrentRenderPassEncoder);

            // - FINISH WITH COMMAND ENCODER
            CommandBuffer* commandBuffer = WGPU.CommandEncoderFinish(CurrentCommandEncoder, null);

            // - PUT ENCODED COMMAND TO QUEUE
            WGPU.QueueSubmit(Queue, 1, &commandBuffer);

            // - PRESENT SURFACE
            WGPU.SurfacePresent(surface);

            // DISPOSE OF RESOURCES
            WGPU.TextureViewRelease(surfaceTextureView);
            WGPU.TextureRelease(surfaceTexture.Texture);
            WGPU.RenderPassEncoderRelease(CurrentRenderPassEncoder);
            WGPU.CommandBufferRelease(commandBuffer);
            WGPU.CommandEncoderRelease(CurrentCommandEncoder);
        }

        public void Dispose()
        {
            OnDispose?.Invoke();

            WGPU.DeviceDestroy(Device);
            Console.WriteLine("WGPU Device Destroyed.");
            WGPU.SurfaceRelease(surface);
            Console.WriteLine("WGPU Surface Released.");
            WGPU.AdapterRelease(adapter);
            Console.WriteLine("WGPU Adapter Released.");
            WGPU.InstanceRelease(instance);
            Console.WriteLine("WGPU Instance Released.");
        }
    }
}
