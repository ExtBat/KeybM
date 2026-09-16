# Keyboard Arrows to Mouse Remapper

A Windows background application that hooks the keyboard at a low level (`WH_KEYBOARD_LL`) and turns a set of keys into mouse cursor control and mouse button clicks. Runs without a window.

## Controls

The program is inactive by default — cursor control is toggled on and off with a dedicated key.

| Key | Action |
|---|---|
| **F12** | Toggle cursor control mode on/off |
| **Esc, Esc** (pressed twice quickly) | Close the program entirely |
| **←** / **→** | Move cursor left / right |
| **↑** / **↓** | Move cursor up / down |
| **Left Shift** (hold) | Fast mode (speed boost) |
| **Right Ctrl** (hold) | Slow mode |
| **Right Shift** | Left mouse button (press/release) |
| **Enter** | Right mouse button (press/release) |
| **+** (regular or numpad) | Mouse wheel up. If Right Ctrl is held — increase base cursor speed |
| **-** (regular or numpad) | Mouse wheel down. If Right Ctrl is held — decrease base cursor speed |

While the mode (F12) is off, all keys behave normally and are not intercepted, except F12 itself (turns the mode on) and the double Esc (closes the program).

## Speed modes

There's a single base cursor speed (`BaseSpeed`) that can be adjusted on the fly. Two modifiers are calculated from it:

- **Fast mode** — Left Shift held. Multiplies speed and acceleration several times over.
- **Slow mode** — Right Ctrl held. Reduces speed and acceleration (roughly by a factor of 3).

Both modes can be held together — their effects multiply.
---
## How it works

- A low-level keyboard hook (`KeyboardHookListener`) intercepts every key press/release before it reaches other applications, and decides whether to pass the event through or suppress it (when the key is used as a control key).
- Direction keys (arrows) don't move the cursor directly. They only toggle flags inside `MotionController`. Actual movement is calculated by a separate timer (`System.Threading.Timer`, ticking every 8 ms) based on elapsed time (`dt`), with smooth acceleration and deceleration — the cursor ramps speed up and down instead of jumping.
- If two opposite arrow keys are held at the same time (e.g. left and right), the cursor doesn't freeze — it moves in the direction of whichever key was pressed **last**. Releasing that key switches movement back to the other one, if it's still held.
- Diagonal movement (e.g. up+left) is normalized so diagonals aren't faster than single-axis movement.
- The cursor is moved via `SendInput` (`MouseSimulator`), so from Windows' and any game's/app's perspective this is real mouse movement, not window-level emulation.
- Mouse clicks (left and right button) also go through `SendInput` and follow key hold state: pressing the key presses the button, releasing the key releases it.
- Pressing Esc twice within 400 ms closes the program. Esc itself is never blocked and keeps working normally everywhere else — the detector only tracks timing between presses without swallowing the event.
- Sounds (`SoundBoard`) are short tones played via `Console.Beep`, tied to specific events (startup, shutdown, mode on/off, mouse button down/up, base speed changes). They play asynchronously, except the shutdown sound, which plays before the process exits.

## Code structure

| File | Purpose |
|---|---|
| `NativeMethods.cs` | All P/Invoke wrappers and Win32 API structures |
| `InputAction.cs` | Enum of logical actions (not keys) |
| `Keybinds.cs` | "Action → key" dictionary and reverse lookup |
| `SoundEvent.cs` | Enum of sound events |
| `SoundBoard.cs` | "Event → sound" dictionary and playback |
| `MouseSimulator.cs` | Sends mouse movement, clicks, and wheel input via `SendInput` |
| `MotionController.cs` | Cursor movement physics: acceleration, deceleration, last-key-pressed priority, base speed |
| `DoubleTapDetector.cs` | Detects a key pressed twice within a given time window |
| `KeyboardHookListener.cs` | Wrapper around the low-level keyboard hook |
| `InputRemapperApp.cs` | Orchestrator — connects the hook, actions, mouse, and sound |
| `Program.cs` | Entry point |

To change the key for an action, edit only `Keybinds.cs`. To change the sound for an event, edit only `SoundBoard.cs`.

## Requirements

- Windows (uses `user32.dll` and `winmm.dll`).
- .NET with `System.Windows.Forms` support (needed for `Application.Run()`'s message loop, which the hook relies on).

## Running

Build and run as a regular console/Windows application. The program doesn't create a visible window and runs in the background until closed (double Esc) or the process is terminated.
