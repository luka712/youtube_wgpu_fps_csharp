using Silk.NET.Input;

namespace FPSGame.Input;

public struct KeyboardState(Dictionary<Key, bool> downKeys, Dictionary<Key, bool> releasedKeys)
{
    /// <summary>
    /// Is key down in current frame. 
    /// </summary>
    public bool IsKeyDown(Key key) => downKeys.ContainsKey(key) && downKeys[key];
    
    /// <summary>
    /// Is key up in current frame.
    /// </summary>
    public bool IsKeyUp(Key key) => !IsKeyDown(key); 
    
    /// <summary>
    /// The key was released in the current frame.
    /// </summary>
    public bool IsKeyReleased(Key key) => releasedKeys.ContainsKey(key) && releasedKeys[key];
}