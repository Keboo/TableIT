using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TableIT.Functions
{
    public class SignalRTestHub : ServerlessHub
    {
        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            ILogger logger)
        {
            logger.LogInformation("Test negotiate login");
            return Negotiate("test-user");
        }

        //[FunctionName("negotiate")]
        //public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
        //{
        //    return Negotiate(req.Headers["x-ms-signalr-user-id"], GetClaims(req.Headers["Authorization"]));
        //}

        [FunctionName("SignalRTest")]
        public async Task SendMessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
        {
            logger.LogInformation($"Receive {message} from {invocationContext.ConnectionId}.");
            await Clients.All.SendAsync("Test", "Some data");
        }

        [FunctionName(nameof(panmessage))]
        public async Task panmessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
        {
            await Clients.All.SendAsync("Test", "Some data");
            await Clients.All.SendAsync("panmessage", message);
            logger.LogInformation($"Receive {message} from {invocationContext.ConnectionId}.");
        }

        [FunctionName(nameof(zoommessage))]
        public async Task zoommessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
        {
            logger.LogInformation($"Receive Zoom {message} from {invocationContext.ConnectionId}.");
            await Clients.All.SendAsync("Test zoom", "Some zoom data");
            await Clients.All.SendAsync("zoommessage", message);
        }

        [FunctionName(nameof(OnConnected))]
        public async Task OnConnected([SignalRTrigger] InvocationContext invocationContext, ILogger logger)
        {
            //await Clients.All.SendAsync(NewConnectionTarget, new NewConnection(invocationContext.ConnectionId));
            logger.LogInformation($"{invocationContext.ConnectionId} has connected");
        }

        [FunctionName(nameof(Broadcast))]
        public async Task Broadcast([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
        {
            //await Clients.All.SendAsync(NewMessageTarget, new NewMessage(invocationContext, message));
            logger.LogInformation($"{invocationContext.ConnectionId} broadcast {message}");
        }

        [FunctionName(nameof(OnDisconnected))]
        public void OnDisconnected([SignalRTrigger] InvocationContext invocationContext)
        {
        }
    }
}
