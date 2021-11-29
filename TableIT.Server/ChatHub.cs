namespace TableIT.Server
{
    public class BroadcastHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly TimeSpan _updateInterval =
            TimeSpan.FromMilliseconds(250);
        private Timer _timer;

        public BroadcastHub()
        {
            _timer = new Timer(Broadcast, null, _updateInterval, _updateInterval);
        }

        public void Broadcast(object state)
        {
            if (Clients is null) return;
            Clients.All.SendCoreAsync("Broadcast", new object[] { DateTime.Now });
        }

    }
}