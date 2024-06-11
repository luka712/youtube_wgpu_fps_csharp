using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace FPSGame
{
    public class Engine
    {
        private IWindow window;

        public void Initialize()
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Size = new Vector2D<int>(1280, 720);
            windowOptions.Title = "FPS Game";

            window = Window.Create(windowOptions);

            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;

            window.Run();
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
    }
}
