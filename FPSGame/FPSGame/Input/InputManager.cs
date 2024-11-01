using Silk.NET.Input;
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
    
    public InputManager(IWindow window)
    {
        IInputContext context = window.CreateInput();
        
        IKeyboard keyboard = context.Keyboards.FirstOrDefault();
        
        keyboard.KeyDown += (_, key, _) =>
        {
            keysDown[key] = true;
        };
        
        keyboard.KeyUp += (_, key, _) =>
        {
            keysDown[key] = false;
            releasedKeys[key] = true;
        };
    }

    public void AfterUpdate()
    {
        foreach(var key in releasedKeys.Keys)
        {
            releasedKeys[key] = false;
        }
    }
    
    public KeyboardState GetKeyboardState()
    {
        return new KeyboardState(keysDown, releasedKeys);
    }
}