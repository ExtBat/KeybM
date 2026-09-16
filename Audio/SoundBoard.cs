internal static class SoundBoard
{
    private readonly struct Tone
    {
        public Tone(int frequency, int durationMs)
        {
            Frequency = frequency;
            DurationMs = durationMs;
        }

        public int Frequency { get; }
        public int DurationMs { get; }
    }

    private static readonly Dictionary<SoundEvent, Tone> Sounds = new()
    {
        [SoundEvent.ProgramStart] = new Tone(880, 90),
        [SoundEvent.ProgramExit] = new Tone(440, 140),
        [SoundEvent.ModeEnabled] = new Tone(1000, 60),
        [SoundEvent.ModeDisabled] = new Tone(600, 60),
        [SoundEvent.LeftMouseDown] = new Tone(1400, 30),
        [SoundEvent.LeftMouseUp] = new Tone(1000, 30),
        [SoundEvent.RightMouseDown] = new Tone(1200, 30),
        [SoundEvent.RightMouseUp] = new Tone(800, 30),
        [SoundEvent.SpeedIncrease] = new Tone(1800, 25),
        [SoundEvent.SpeedDecrease] = new Tone(500, 25),
    };

    public static void Play(SoundEvent soundEvent)
    {
        if (!Sounds.TryGetValue(soundEvent, out var tone))
        {
            return;
        }

        Task.Run(() => SafeBeep(tone));
    }

    public static void PlayBlocking(SoundEvent soundEvent)
    {
        if (!Sounds.TryGetValue(soundEvent, out var tone))
        {
            return;
        }

        SafeBeep(tone);
    }

    private static void SafeBeep(Tone tone)
    {
        try
        {
            Console.Beep(tone.Frequency, tone.DurationMs);
        }
        catch (Exception)
        {
        }
    }
}