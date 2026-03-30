using Avalonia.Input;
using System.Collections.Generic;

namespace SkyForce.Core;

public class InputManager
{
    public HashSet<Key> PressedKeys { get; } = new HashSet<Key>();

    public void KeyDown(Key key) => PressedKeys.Add(key);
    public void KeyUp(Key key) => PressedKeys.Remove(key);
}