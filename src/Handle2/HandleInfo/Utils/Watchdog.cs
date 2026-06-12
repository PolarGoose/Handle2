namespace Handle2.HandleInfo.Utils;

internal sealed class Watchdog : IDisposable
{
    private readonly Action onTriggered;
    private Timer? timer;
    private readonly TimeSpan timeout;

    public Watchdog(Action onTriggered, TimeSpan timeout)
    {
        this.onTriggered = onTriggered;
        this.timeout = timeout;
    }

    public void Arm()
    {
        timer = new Timer(OnTimerElapsed, null, timeout, Timeout.InfiniteTimeSpan);
    }

    public void Disarm()
    {
        timer?.Dispose();
        timer = null;
    }

    private void OnTimerElapsed(object? state)
    {
        onTriggered();
    }

    void IDisposable.Dispose()
    {
        timer?.Dispose();
        timer = null;
    }
}
