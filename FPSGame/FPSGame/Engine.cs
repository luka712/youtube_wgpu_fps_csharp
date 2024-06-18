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
        
        /// <summary>
        /// Initializes the engine and starts the render loop.
        /// </summary>
        public void Initialize()
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Size = new Vector2D<int>(1280, 720);
            windowOptions.Title = "FPS Game";
            windowOptions.API = GraphicsAPI.None;

            window = Window.Create(windowOptions);
            window.Initialize();
            
            // Prepare WebGPU.
            CreateApi();
            CreateInstance();
            CreateSurface();
            CreateAdapter();
            CreateDevice();
            ConfigureDebugCallback();
            ConfigureSurface();

            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;

            window.Run();
        }

        private void CreateApi()
        {
            wgpu = WebGPU.GetApi();
            Console.WriteLine("WebGPU Api retrieved.");
        }

        private void CreateInstance()
        {
            InstanceDescriptor descriptor = new InstanceDescriptor();
            instance = wgpu.CreateInstance(in descriptor);
            Console.WriteLine("WebGPU instance created.");
        }
        
        private void CreateSurface()
        {
            surface = window.CreateWebGPUSurface(wgpu, instance);
            Console.WriteLine("WebGPU surface created.");
        }

        private void CreateAdapter()
        {
            RequestAdapterOptions options = new RequestAdapterOptions();
            options.CompatibleSurface = surface;
            options.PowerPreference = PowerPreference.HighPerformance;
            options.BackendType = BackendType.Vulkan;

            PfnRequestAdapterCallback callback = PfnRequestAdapterCallback.From((status, wgpuAdapter, msgPtr, userData) =>
            {
                if (status == RequestAdapterStatus.Success)
                {
                    this.adapter = wgpuAdapter;
                    Console.WriteLine("WebGPU adapter retrieved.");
                }
                else
                {
                    string error = Marshal.PtrToStringBSTR((IntPtr)msgPtr);
                    Console.WriteLine(error);
                }
            });
            
            wgpu.InstanceRequestAdapter(instance, in options, callback, null);
        }

        private void CreateDevice()
        {
            DeviceDescriptor descriptor = new DeviceDescriptor();
            descriptor.Label = (byte*)Marshal.StringToHGlobalAnsi("Device");
            
            PfnRequestDeviceCallback callback = PfnRequestDeviceCallback.From((status, wgpuDevice, msgPtr, userData) =>
            {
                if (status == RequestDeviceStatus.Success)
                {
                    this.device = wgpuDevice;
                    Console.WriteLine("Device retrieved.");
                }
                else
                {
                    string error = Marshal.PtrToStringBSTR((IntPtr)msgPtr);
                    Console.WriteLine(error);
                }
            });

            wgpu.AdapterRequestDevice(adapter, in descriptor, callback, null);
        }

        private void ConfigureDebugCallback()
        {
            PfnErrorCallback callback = PfnErrorCallback.From((errorType, msgPtr, userData ) =>
            {
                string msg = Marshal.PtrToStringBSTR((IntPtr)msgPtr);
                Console.WriteLine($"WGPU Error: {msg}");
            });
            
            wgpu.DeviceSetUncapturedErrorCallback(device, callback, null);
            Console.WriteLine("Error callback set.");
        }

        private void ConfigureSurface()
        {
            SurfaceConfiguration configuration = new SurfaceConfiguration();
            configuration.Device = device;
            configuration.Usage = TextureUsage.RenderAttachment;
            configuration.Format = TextureFormat.Bgra8Unorm;
            configuration.PresentMode = PresentMode.Fifo;
            configuration.Width = (uint) window.Size.X;
            configuration.Height = (uint) window.Size.Y;
            
            wgpu.SurfaceConfigure(surface, in configuration);

            Console.WriteLine("Surface configured.");
        }
        
        private void OnLoad()
        {
        }

        private void OnUpdate(double dt)
        {
        }

        private void OnRender(double dt)
        {
         
        }

        public void Dispose()
        {
            wgpu.DeviceDestroy(device);
            Console.WriteLine("Device destroyed.");
            wgpu.SurfaceRelease(surface);
            Console.WriteLine("Surface released.");
            wgpu.AdapterRelease(adapter);
            Console.WriteLine("Adapter released.");
            wgpu.InstanceRelease(instance);
            Console.WriteLine("Instance released");
        }
    }
}
