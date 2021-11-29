namespace TableIT.Server
{
    public interface IBroadcast
    {
        void Notify(DateTime now);
    }

    public class BroadcastHub : Microsoft.AspNetCore.SignalR.Hub<IBroadcast>
    {
        public void NotifyAll()
        {
            
            if (Clients is { } clients)
            {
                clients.All.Notify(DateTime.Now);
            }
        }
    }
}