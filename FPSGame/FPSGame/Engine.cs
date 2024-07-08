using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;

namespace FPSGame
{
    public unsafe class Engine : IDisposable
    {
        private IWindow window;

        private Instance* instance;
        private Surface* surface;
        private Adapter* adapter;

        private CommandEncoder* currentCommandEncoder;

        private SurfaceTexture surfaceTexture;
        private TextureView* surfaceTextureView;

        public event Action OnInitialize;
        public event Action OnRender;
        public event Action OnDispose;

        public WebGPU WGPU {get; private set; }

        public Device* Device { get; private set; }

        public Queue* Queue { get; private set; }

        public TextureFormat PreferredTextureFormat => TextureFormat.Bgra8Unorm;

        public RenderPassEncoder* CurrentRenderPassEncoder { get; private set; }

        public void Initialize()
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Size = new Vector2D<int>(1280, 720);
            windowOptions.Title = "FPS Game";
            windowOptions.API = GraphicsAPI.None;

            window = Window.Create(windowOptions);
            window.Initialize();

            // Setup WGPU.
            CreateApi();
            CreateInstance();
            CreateSurface();
            CreateAdapter();
            CreateDevice();
            ConfigureSurface();
            ConfigureDebugCallback();

            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += Window_OnRender;

            // - QUEUE
            Queue = WGPU.DeviceGetQueue(Device);

            OnInitialize?.Invoke();

            window.Run();
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
            surface = window.CreateWebGPUSurface(WGPU, instance);
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
            configuration.Width = (uint)window.Size.X;
            configuration.Height = (uint)window.Size.Y;
            configuration.Format = PreferredTextureFormat;
            configuration.PresentMode = PresentMode.Immediate;
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

        private void OnLoad()
        {
        }

        private void OnUpdate(double dt)
        {
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
            currentCommandEncoder = WGPU.DeviceCreateCommandEncoder(Device, null);

            // - SURFACE TEXTURE
            WGPU.SurfaceGetCurrentTexture(surface, ref surfaceTexture);
            surfaceTextureView = WGPU.TextureCreateView(surfaceTexture.Texture, null);

            // - RENDER PASS ENCODER
            RenderPassColorAttachment* colorAttachments = stackalloc RenderPassColorAttachment[1];
            colorAttachments[0].View = surfaceTextureView;
            colorAttachments[0].LoadOp = LoadOp.Clear;
            colorAttachments[0].ClearValue = new Color(0.1, 0.9, 0.9, 1.0);
            colorAttachments[0].StoreOp = StoreOp.Store;

            RenderPassDescriptor renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.ColorAttachments = colorAttachments;
            renderPassDescriptor.ColorAttachmentCount = 1;

            CurrentRenderPassEncoder = WGPU.CommandEncoderBeginRenderPass(currentCommandEncoder, renderPassDescriptor);
        }

        private void AfterRender()
        {
            // - END RENDER PASS
            WGPU.RenderPassEncoderEnd(CurrentRenderPassEncoder);

            // - FINISH WITH COMMAND ENCODER
            CommandBuffer* commandBuffer = WGPU.CommandEncoderFinish(currentCommandEncoder, null);

            // - PUT ENCODED COMMAND TO QUEUE
            WGPU.QueueSubmit(Queue, 1, &commandBuffer);

            // - PRESENT SURFACE
            WGPU.SurfacePresent(surface);

            // DISPOSE OF RESOURCES
            WGPU.TextureViewRelease(surfaceTextureView);
            WGPU.TextureRelease(surfaceTexture.Texture);
            WGPU.RenderPassEncoderRelease(CurrentRenderPassEncoder);
            WGPU.CommandBufferRelease(commandBuffer);
            WGPU.CommandEncoderRelease(currentCommandEncoder);
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
