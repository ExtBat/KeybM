internal static class Keybinds
{
    public static readonly Dictionary<InputAction, int> ActionToKey = new()
    {
        [InputAction.ToggleMode] = NativeMethods.VK_F12,
        [InputAction.Exit] = NativeMethods.VK_ESCAPE,
        [InputAction.MoveLeft] = NativeMethods.VK_LEFT,
        [InputAction.MoveUp] = NativeMethods.VK_UP,
        [InputAction.MoveRight] = NativeMethods.VK_RIGHT,
        [InputAction.MoveDown] = NativeMethods.VK_DOWN,
        [InputAction.Boost] = NativeMethods.VK_LSHIFT,
        [InputAction.Slow] = NativeMethods.VK_RCONTROL,
        [InputAction.LeftClick] = NativeMethods.VK_RSHIFT,
        [InputAction.RightClick] = NativeMethods.VK_RETURN,
        [InputAction.SpeedIncrease] = NativeMethods.VK_OEM_PLUS,
        [InputAction.SpeedDecrease] = NativeMethods.VK_OEM_MINUS,
    };

    private static readonly Dictionary<int, InputAction> SecondaryKeyToAction = new()
    {
        [NativeMethods.VK_ADD] = InputAction.SpeedIncrease,
        [NativeMethods.VK_SUBTRACT] = InputAction.SpeedDecrease,
    };

    private static readonly Dictionary<int, InputAction> KeyToAction = BuildReverse();

    private static Dictionary<int, InputAction> BuildReverse()
    {
        var map = new Dictionary<int, InputAction>();

        foreach (var pair in ActionToKey)
        {
            map[pair.Value] = pair.Key;
        }

        foreach (var pair in SecondaryKeyToAction)
        {
            map[pair.Key] = pair.Value;
        }

        return map;
    }

    public static bool TryGetAction(int virtualKey, out InputAction action)
    {
        return KeyToAction.TryGetValue(virtualKey, out action);
    }
}