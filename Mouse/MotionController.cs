internal sealed class MotionController
{
    private sealed class AxisState
    {
        private bool negativeHeld;
        private bool positiveHeld;
        private int negativeOrder;
        private int positiveOrder;

        public void SetNegative(bool down, int order)
        {
            if (down && !negativeHeld)
            {
                negativeOrder = order;
            }

            negativeHeld = down;
        }

        public void SetPositive(bool down, int order)
        {
            if (down && !positiveHeld)
            {
                positiveOrder = order;
            }

            positiveHeld = down;
        }

        public double Direction
        {
            get
            {
                if (negativeHeld && positiveHeld)
                {
                    return positiveOrder > negativeOrder ? 1 : -1;
                }

                if (negativeHeld)
                {
                    return -1;
                }

                if (positiveHeld)
                {
                    return 1;
                }

                return 0;
            }
        }

        public void Reset()
        {
            negativeHeld = false;
            positiveHeld = false;
        }
    }

    private const double AccelToSpeedRatio = 6000.0 / 1100.0;
    private const double BoostMultiplier = 2.3;
    private const double SlowMultiplier = 0.35;
    private const double MinBaseSpeed = 100;
    private const double MaxBaseSpeed = 5000;
    private const double BaseSpeedStep = 100;

    private readonly AxisState horizontal = new();
    private readonly AxisState vertical = new();
    private int pressOrderCounter;

    private double vx;
    private double vy;
    private double accumX;
    private double accumY;

    public double BaseSpeed { get; private set; } = 1100;
    public bool BoostActive { get; set; }
    public bool SlowActive { get; set; }

    public void SetKey(InputAction action, bool down)
    {
        int order = down ? ++pressOrderCounter : 0;

        switch (action)
        {
            case InputAction.MoveLeft:
                horizontal.SetNegative(down, order);
                break;
            case InputAction.MoveRight:
                horizontal.SetPositive(down, order);
                break;
            case InputAction.MoveUp:
                vertical.SetNegative(down, order);
                break;
            case InputAction.MoveDown:
                vertical.SetPositive(down, order);
                break;
        }
    }

    public void IncreaseBaseSpeed()
    {
        BaseSpeed = Math.Min(MaxBaseSpeed, BaseSpeed + BaseSpeedStep);
    }

    public void DecreaseBaseSpeed()
    {
        BaseSpeed = Math.Max(MinBaseSpeed, BaseSpeed - BaseSpeedStep);
    }

    public (int dx, int dy) Tick(double dt)
    {
        double dirX = horizontal.Direction;
        double dirY = vertical.Direction;

        if (dirX != 0 && dirY != 0)
        {
            dirX *= 0.7071067811865476;
            dirY *= 0.7071067811865476;
        }

        double multiplier = 1.0;

        if (BoostActive)
        {
            multiplier *= BoostMultiplier;
        }

        if (SlowActive)
        {
            multiplier *= SlowMultiplier;
        }

        double maxSpeed = BaseSpeed * multiplier;
        double accel = maxSpeed * AccelToSpeedRatio;

        vx = MoveTowards(vx, dirX * maxSpeed, accel * dt);
        vy = MoveTowards(vy, dirY * maxSpeed, accel * dt);

        accumX += vx * dt;
        accumY += vy * dt;

        int moveX = (int)accumX;
        int moveY = (int)accumY;
        accumX -= moveX;
        accumY -= moveY;

        return (moveX, moveY);
    }

    public void Reset()
    {
        horizontal.Reset();
        vertical.Reset();
        vx = 0;
        vy = 0;
        accumX = 0;
        accumY = 0;
    }

    private static double MoveTowards(double current, double target, double maxDelta)
    {
        double diff = target - current;

        if (Math.Abs(diff) <= maxDelta)
        {
            return target;
        }

        return current + Math.Sign(diff) * maxDelta;
    }
}