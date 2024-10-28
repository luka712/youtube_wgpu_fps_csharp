using Silk.NET.Input;

namespace FPSGame.Input;


public struct KeyboardState(Dictionary<Key, bool> downKeys, Dictionary<Key, bool> releasedKeys)
{
    /// <summary>
    /// Is key down in current frame.
    /// </summary>
    public bool IsKeyDown(Key key)
    {
        return downKeys.ContainsKey(key) && downKeys[key];
    }
    
    /// <summary>
    /// Is key up in current frame.
    /// </summary>
    public bool IsKeyUp(Key key)
    {
        return !IsKeyDown(key);
    }
    
    /// <summary>
    /// The key that was released in current frame.
    /// </summary>
    public bool IsKeyReleased(Key key)
    {
        return releasedKeys.ContainsKey(key) && releasedKeys[key];
    }
}