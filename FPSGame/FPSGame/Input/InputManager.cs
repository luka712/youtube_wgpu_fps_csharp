using Silk.NET.Input;
using Silk.NET.Windowing;

namespace FPSGame.Input;

public class InputManager
{
    /// <summary>
    /// Keys that are pressed in the current frame.
    /// </summary>
    private Dictionary<Key, bool> keysDown = new();
    
    /// <summary>
    /// Keys that are released in the current frame.
    /// </summary>
    private Dictionary<Key, bool> releasedKeys = new();

    public InputManager(IWindow window)
    {
        IInputContext inputContext = window.CreateInput();

        IKeyboard keyboard = inputContext.Keyboards[0];
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
        foreach(Key key in releasedKeys.Keys)
        {
            releasedKeys[key] = false;
        }
    }
    
    public KeyboardState GetKeyboardState()
    {
        return new KeyboardState(keysDown, releasedKeys);
    }
}