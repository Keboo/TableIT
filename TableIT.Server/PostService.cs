namespace TableIT.Server
{
    public class PostService : IHostedService
    {
        private readonly TimeSpan _updateInterval =
            TimeSpan.FromMilliseconds(250);
        private Timer _timer;

        public BroadcastHub Hub { get; }

        public PostService(BroadcastHub hub)
        {
            Hub = hub;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Broadcast, null, _updateInterval, _updateInterval);
            return Task.CompletedTask;
        }

        public void Broadcast(object state)
        {
            Hub.NotifyAll();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
