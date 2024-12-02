using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace FPSGame.Input;

public class InputManager
{
    /// <summary>
    /// Keys that are pressed in current frame.
    /// </summary>
    private Dictionary<Key, bool> keysDown = new();
    
    /// <summary>
    /// Keys that are released in current frame.
    /// </summary>
    private Dictionary<Key, bool> releasedKeys = new();

    private Vector2D<float> previousMousePosition = new();
    private Vector2D<float> deltaMouse = new();

    public InputManager(IWindow window)
    {
        IInputContext context = window.CreateInput();

        IKeyboard keyboard = context.Keyboards.FirstOrDefault();
        IMouse mouse = context.Mice.FirstOrDefault();

        keyboard.KeyDown += (_, key, _) => { keysDown[key] = true; };

        keyboard.KeyUp += (_, key, _) =>
        {
            keysDown[key] = false;
            releasedKeys[key] = true;
        };

        mouse.MouseMove += (_, currentMousePosition) =>
        {
            deltaMouse.X = currentMousePosition.X - previousMousePosition.X;
            deltaMouse.Y = currentMousePosition.Y - previousMousePosition.Y;
            previousMousePosition = new Vector2D<float>(currentMousePosition.X, currentMousePosition.Y);
        };
    }

    public void AfterUpdate()
    {
        foreach(var key in releasedKeys.Keys)
        {
            releasedKeys[key] = false;
        }
        deltaMouse = Vector2D<float>.Zero;
    }
    
    public KeyboardState GetKeyboardState()
    {
        return new KeyboardState(keysDown, releasedKeys);
    }

    public MouseState GetMouseState()
    {
        return new MouseState()
        {
            DeltaX = deltaMouse.X,
            DeltaY = deltaMouse.Y
        };
    }
}