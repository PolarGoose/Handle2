namespace Handle2.HandleInfo.Utils;

internal static class WorkerThreadWithDeadLockDetection
{
    // Returns:
    // * true in case the action finished successfully
    // * false in case of a deadlock
    public static bool Run(TimeSpan deadLockTimeout, Action<Watchdog> action)
    {
        var deadlockDetected = new TaskCompletionSource<bool>();
        using var watchdog = new Watchdog(() => deadlockDetected.TrySetResult(true), deadLockTimeout);
        var workerTask = Task.Factory.StartNew(() => action(watchdog), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        var completedTask = Task.WaitAny(workerTask, deadlockDetected.Task);
        return completedTask == 0;
    }
}
