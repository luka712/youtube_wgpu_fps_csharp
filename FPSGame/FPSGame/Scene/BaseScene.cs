namespace FPSGame.Scene;

public abstract class BaseScene : IDisposable
{
    public abstract void Initialize();

    public abstract void Render();
    
    public abstract void Dispose();
}