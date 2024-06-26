using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;

namespace FPSGame
{
    public unsafe class Engine : IDisposable
    {
        private IWindow window;
        private WebGPU wgpu;
        private Instance* instance;
        private Surface* surface;
        private Adapter* adapter;
        private Device* device;

        private Queue* queue;
        private CommandEncoder* currentCommandEncoder;
        private RenderPassEncoder* currentRenderPassEncoder;
        private SurfaceTexture surfaceTexture;
        private TextureView* surfaceTextureView;

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
            window.Render += OnRender;

            window.Run();
        }

        private void CreateApi()
        {
            wgpu = WebGPU.GetApi();
            Console.WriteLine("Created WGPU API.");
        }

        private void CreateInstance()
        {
            InstanceDescriptor descriptor = new InstanceDescriptor();
            instance = wgpu.CreateInstance(descriptor);
            Console.WriteLine("Created WGPU Instance.");
        }

        private void CreateSurface()
        {
            surface = window.CreateWebGPUSurface(wgpu, instance);
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

            wgpu.InstanceRequestAdapter(instance, options, callback, null);
        }

        private void CreateDevice()
        {
            PfnRequestDeviceCallback callback = PfnRequestDeviceCallback.From(
                (status, wgpuDevice, msgPtr, userDataPtr) =>
                {
                    if (status == RequestDeviceStatus.Success)
                    {
                        this.device = wgpuDevice;
                        Console.WriteLine("Retrieved WGPU Device.");
                    }
                    else
                    {
                        string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                        Console.WriteLine($"Error while retrieving WGPU Device: {msg}");
                    }
                });

            DeviceDescriptor descriptor = new DeviceDescriptor();
            wgpu.AdapterRequestDevice(adapter, descriptor, callback, null);
        }

        private void ConfigureSurface()
        {
            SurfaceConfiguration configuration = new SurfaceConfiguration();
            configuration.Device = device;
            configuration.Width = (uint)window.Size.X;
            configuration.Height = (uint)window.Size.Y;
            configuration.Format = TextureFormat.Bgra8Unorm;
            configuration.PresentMode = PresentMode.Immediate;
            configuration.Usage = TextureUsage.RenderAttachment;

            wgpu.SurfaceConfigure(surface, configuration);
        }

        private void ConfigureDebugCallback()
        {
            PfnErrorCallback callback = PfnErrorCallback.From((type, msgPtr, userDataPtr) =>
            {
                string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                Console.WriteLine($"WGPU Unhandled error callback: {msg}");
            });

            wgpu.DeviceSetUncapturedErrorCallback(device, callback, null);
            Console.WriteLine("WGPU Debug Callback Configured.");
        }

        private void OnLoad()
        {
        }

        private void OnUpdate(double dt)
        {
        }

        private void OnRender(double dt)
        {
            BeforeRender();
            
            // TODO: draw here.

            AfterRender();
        }

        private void BeforeRender()
        {
            // - QUEUE
            queue = wgpu.DeviceGetQueue(device);

            // - COMMAND ENCODER
            currentCommandEncoder = wgpu.DeviceCreateCommandEncoder(device, null);

            // - SURFACE TEXTURE
            wgpu.SurfaceGetCurrentTexture(surface, ref surfaceTexture);
            surfaceTextureView = wgpu.TextureCreateView(surfaceTexture.Texture, null);

            // - RENDER PASS ENCODER
            RenderPassColorAttachment* colorAttachments = stackalloc RenderPassColorAttachment[1];
            colorAttachments[0].View = surfaceTextureView;
            colorAttachments[0].LoadOp = LoadOp.Clear;
            colorAttachments[0].ClearValue = new Color(0.1, 0.9, 0.9, 1.0);
            colorAttachments[0].StoreOp = StoreOp.Store;

            RenderPassDescriptor renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.ColorAttachments = colorAttachments;
            renderPassDescriptor.ColorAttachmentCount = 1;

            currentRenderPassEncoder = wgpu.CommandEncoderBeginRenderPass(currentCommandEncoder, renderPassDescriptor);
        }

        private void AfterRender()
        {
            // - END RENDER PASS
            wgpu.RenderPassEncoderEnd(currentRenderPassEncoder);

            // - FINISH WITH COMMAND ENCODER
            CommandBuffer* commandBuffer = wgpu.CommandEncoderFinish(currentCommandEncoder, null);

            // - PUT ENCODED COMMAND TO QUEUE
            wgpu.QueueSubmit(queue, 1, &commandBuffer);

            // - PRESENT SURFACE
            wgpu.SurfacePresent(surface);

            // DISPOSE OF RESOURCES
            wgpu.TextureViewRelease(surfaceTextureView);
            wgpu.TextureRelease(surfaceTexture.Texture);
            wgpu.RenderPassEncoderRelease(currentRenderPassEncoder);
            wgpu.CommandBufferRelease(commandBuffer);
            wgpu.CommandEncoderRelease(currentCommandEncoder);
        }

     
        public void Dispose()
        {
            wgpu.DeviceDestroy(device);
            Console.WriteLine("WGPU Device Destroyed.");
            wgpu.SurfaceRelease(surface);
            Console.WriteLine("WGPU Surface Released.");
            wgpu.AdapterRelease(adapter);
            Console.WriteLine("WGPU Adapter Released.");
            wgpu.InstanceRelease(instance);
            Console.WriteLine("WGPU Instance Released.");

        }
    }
}