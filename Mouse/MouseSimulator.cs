using System.Runtime.InteropServices;

internal sealed class MouseSimulator
{
    public void Move(int dx, int dy)
    {
        if (dx == 0 && dy == 0)
        {
            return;
        }

        Send(NativeMethods.BuildMouseInput(dx, dy, 0, NativeMethods.MOUSEEVENTF_MOVE));
    }

    public void Wheel(int amount)
    {
        Send(NativeMethods.BuildMouseInput(0, 0, unchecked((uint)amount), NativeMethods.MOUSEEVENTF_WHEEL));
    }

    public void LeftButton(bool down)
    {
        Send(NativeMethods.BuildMouseInput(0, 0, 0, down ? NativeMethods.MOUSEEVENTF_LEFTDOWN : NativeMethods.MOUSEEVENTF_LEFTUP));
    }

    public void RightButton(bool down)
    {
        Send(NativeMethods.BuildMouseInput(0, 0, 0, down ? NativeMethods.MOUSEEVENTF_RIGHTDOWN : NativeMethods.MOUSEEVENTF_RIGHTUP));
    }

    private static void Send(NativeMethods.INPUT input)
    {
        NativeMethods.SendInput(1, new[] { input }, Marshal.SizeOf<NativeMethods.INPUT>());
    }
}