using System.Diagnostics;
using System.Runtime.InteropServices;

internal sealed class KeyboardHookListener : IDisposable
{
    private readonly NativeMethods.LowLevelKeyboardProc proc;
    private readonly Func<int, bool, bool> handler;
    private IntPtr hookHandle;

    public KeyboardHookListener(Func<int, bool, bool> handler)
    {
        this.handler = handler;
        proc = HookCallback;
    }

    public void Install()
    {
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule!;

        hookHandle = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_KEYBOARD_LL,
            proc,
            NativeMethods.GetModuleHandle(module.ModuleName!),
            0);

        if (hookHandle == IntPtr.Zero)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
        {
            return NativeMethods.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        var info = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
        int vk = unchecked((int)info.vkCode);
        bool down = wParam == (IntPtr)NativeMethods.WM_KEYDOWN || wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN;
        bool up = wParam == (IntPtr)NativeMethods.WM_KEYUP || wParam == (IntPtr)NativeMethods.WM_SYSKEYUP;

        if (!down && !up)
        {
            return NativeMethods.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        bool suppress = handler(vk, down);

        return suppress ? (IntPtr)1 : NativeMethods.CallNextHookEx(hookHandle, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (hookHandle != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(hookHandle);
            hookHandle = IntPtr.Zero;
        }
    }
}