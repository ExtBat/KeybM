internal sealed class DoubleTap
{
    private readonly double windowSeconds;
    private double lastTimestamp = double.NegativeInfinity;

    public DoubleTap(double windowSeconds)
    {
        this.windowSeconds = windowSeconds;
    }

    public bool RegisterPress(double timestamp)
    {
        bool isDoubleTap = timestamp - lastTimestamp <= windowSeconds;
        lastTimestamp = isDoubleTap ? double.NegativeInfinity : timestamp;
        return isDoubleTap;
    }
}