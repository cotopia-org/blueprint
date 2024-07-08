namespace blueprint.core
{
    public static class SafeloopThreadPool
    {
        public static async Task ExecuteAsync(Action action, int timeoutMilliseconds)
        {
            using (var cts = new CancellationTokenSource())
            {
                var task = Task.Run(action, cts.Token);
                var delay = Task.Delay(timeoutMilliseconds, cts.Token);

                var done = await Task.WhenAny(task, delay);
                if (done == delay)
                {
                    cts.Cancel();
                    throw new TimeoutException("The action exceeded the time limit.");
                }

                await task; // Await the task to propagate exceptions
            }
        }
    }
}
