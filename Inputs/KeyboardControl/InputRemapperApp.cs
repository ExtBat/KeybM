using System.Diagnostics;
using Timer = System.Threading.Timer;

internal sealed class InputRemapperApp : IDisposable
{
    private const double DoubleEscWindowSeconds = 0.4;
    private const int TimerPeriodMs = 8;

    private readonly MotionController motion = new();
    private readonly MouseSimulator mouse = new();
    private readonly DoubleTap escDetector = new(DoubleEscWindowSeconds);
    private readonly Stopwatch clock = new();
    private readonly object sync = new();

    private KeyboardHookListener? hook;
    private Timer? motionTimer;

    private bool enabled;
    private bool leftMouseDown;
    private bool rightMouseDown;
    private double lastTime;

    public void Run()
    {
        NativeMethods.TimeBeginPeriod(1);
        SoundBoard.Play(SoundEvent.ProgramStart);

        hook = new KeyboardHookListener(HandleKey);
        hook.Install();

        clock.Start();
        lastTime = clock.Elapsed.TotalSeconds;
        motionTimer = new Timer(OnTick, null, 0, TimerPeriodMs);

        Application.Run();
    }

    private bool HandleKey(int vk, bool down)
    {
        if (vk == Keybinds.ActionToKey[InputAction.Exit] && down)
        {
            if (escDetector.RegisterPress(clock.Elapsed.TotalSeconds))
            {
                Application.Exit();
            }

            return false;
        }

        lock (sync)
        {
            if (!enabled)
            {
                if (vk == Keybinds.ActionToKey[InputAction.ToggleMode] && down)
                {
                    enabled = true;
                    SoundBoard.Play(SoundEvent.ModeEnabled);
                    return true;
                }

                return false;
            }

            if (!Keybinds.TryGetAction(vk, out var action))
            {
                return false;
            }

            return Dispatch(action, down);
        }
    }

    private bool Dispatch(InputAction action, bool down)
    {
        switch (action)
        {
            case InputAction.ToggleMode:
                if (!down)
                {
                    return false;
                }

                enabled = false;
                SoundBoard.Play(SoundEvent.ModeDisabled);
                ReleaseAll();
                return true;

            case InputAction.Boost:
                motion.BoostActive = down;
                return true;

            case InputAction.Slow:
                motion.SlowActive = down;
                return true;

            case InputAction.LeftClick:
                if (down && !leftMouseDown)
                {
                    leftMouseDown = true;
                    mouse.LeftButton(true);
                    SoundBoard.Play(SoundEvent.LeftMouseDown);
                }
                else if (!down && leftMouseDown)
                {
                    leftMouseDown = false;
                    mouse.LeftButton(false);
                    SoundBoard.Play(SoundEvent.LeftMouseUp);
                }

                return true;

            case InputAction.RightClick:
                if (down && !rightMouseDown)
                {
                    rightMouseDown = true;
                    mouse.RightButton(true);
                    SoundBoard.Play(SoundEvent.RightMouseDown);
                }
                else if (!down && rightMouseDown)
                {
                    rightMouseDown = false;
                    mouse.RightButton(false);
                    SoundBoard.Play(SoundEvent.RightMouseUp);
                }

                return true;

            case InputAction.MoveLeft:
            case InputAction.MoveUp:
            case InputAction.MoveRight:
            case InputAction.MoveDown:
                motion.SetKey(action, down);
                return true;

            case InputAction.SpeedIncrease:
                if (down)
                {
                    SoundBoard.Play(SoundEvent.SpeedIncrease);

                    if (motion.SlowActive)
                    {
                        motion.IncreaseBaseSpeed();
                    }
                    else
                    {
                        mouse.Wheel(120);
                    }
                }

                return true;

            case InputAction.SpeedDecrease:
                if (down)
                {
                    SoundBoard.Play(SoundEvent.SpeedDecrease);

                    if (motion.SlowActive)
                    {
                        motion.DecreaseBaseSpeed();
                    }
                    else
                    {
                        mouse.Wheel(-120);
                    }
                }

                return true;

            default:
                return false;
        }
    }

    private void OnTick(object? state)
    {
        lock (sync)
        {
            double now = clock.Elapsed.TotalSeconds;
            double dt = now - lastTime;
            lastTime = now;

            if (!enabled)
            {
                return;
            }

            if (dt <= 0 || dt > 0.05)
            {
                dt = 0.016;
            }

            var (dx, dy) = motion.Tick(dt);

            if (dx != 0 || dy != 0)
            {
                mouse.Move(dx, dy);
            }
        }
    }

    private void ReleaseAll()
    {
        if (leftMouseDown)
        {
            mouse.LeftButton(false);
            leftMouseDown = false;
        }

        if (rightMouseDown)
        {
            mouse.RightButton(false);
            rightMouseDown = false;
        }

        motion.BoostActive = false;
        motion.SlowActive = false;
        motion.Reset();
    }

    public void Dispose()
    {
        motionTimer?.Dispose();
        hook?.Dispose();
        NativeMethods.TimeEndPeriod(1);
        SoundBoard.PlayBlocking(SoundEvent.ProgramExit);
    }
}