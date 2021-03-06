#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace TableIT.Functions;

public class TableItHub : ServerlessHub
{
    private const string TableIdClaimType = "tableid";

    [FunctionName("negotiate")]
    public SignalRConnectionInfo Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        ILogger logger)
    {
        string? userName = null;
        IList<Claim> claims = Array.Empty<Claim>();
        if (req.Headers.ContainsKey("Authorization") &&
            req.Headers["Authorization"] is { } authHeader)
        {
            claims = GetClaims(authHeader);
            userName = claims.FirstOrDefault(x => x.Type == TableIdClaimType)?.Value;
        }
        else if (req.Query.TryGetValue(TableIdClaimType, out var tableId) && 
            !string.IsNullOrWhiteSpace(tableId))
        {
            userName = tableId;
        }

        logger.LogInformation($"{nameof(Negotiate)}: claims {string.Join(",", claims.Select(x => $"[{x.Type}, {x.Value}]"))}");

        return Negotiate(userName ?? $"annoymous-user-{Guid.NewGuid()}");
    }

    [FunctionName(nameof(PanMessage))]
    public async Task PanMessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
    {
        logger.LogInformation($"Receive {nameof(PanMessage)} from {invocationContext.ConnectionId} for user {invocationContext.UserId}.");
        await Clients.User(invocationContext.UserId).SendAsync(nameof(PanMessage).ToLowerInvariant(), message);
    }

    [FunctionName(nameof(ZoomMessage))]
    public async Task ZoomMessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
    {
        logger.LogInformation($"Receive {nameof(ZoomMessage)} from {invocationContext.ConnectionId} for user {invocationContext.UserId}.");
        await Clients.User(invocationContext.UserId).SendAsync(nameof(ZoomMessage).ToLowerInvariant(), message);
    }

    [FunctionName(nameof(TableMessage))]
    public async Task TableMessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
    {
        logger.LogInformation($"Receive {nameof(TableMessage)} from {invocationContext.ConnectionId} for user {invocationContext.UserId}.");
        await Clients.User(invocationContext.UserId).SendAsync(nameof(TableMessage).ToLowerInvariant(), message);
    }

    [FunctionName(nameof(RemoteMessage))]
    public async Task RemoteMessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
    {
        logger.LogInformation($"Receive {nameof(RemoteMessage)} from {invocationContext.ConnectionId} for user {invocationContext.UserId}.");
        await Clients.User(invocationContext.UserId).SendAsync(nameof(RemoteMessage).ToLowerInvariant(), message);
    }

    [FunctionName(nameof(RequestMessage))]
    public async Task RequestMessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
    {
        logger.LogInformation($"Receive {nameof(RequestMessage)} from {invocationContext.ConnectionId} for user {invocationContext.UserId}.");
        await Clients.User(invocationContext.UserId).SendAsync(nameof(RequestMessage).ToLowerInvariant(), message);
    }

    [FunctionName(nameof(ResponseMessage))]
    public async Task ResponseMessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
    {
        logger.LogInformation($"Receive {nameof(ResponseMessage)} from {invocationContext.ConnectionId} for user {invocationContext.UserId}.");
        await Clients.User(invocationContext.UserId).SendAsync(nameof(ResponseMessage).ToLowerInvariant(), message);
    }

    [FunctionName(nameof(OnConnected))]
    public void OnConnected([SignalRTrigger] InvocationContext invocationContext, ILogger logger)
    {
        //await Clients.All.SendAsync(NewConnectionTarget, new NewConnection(invocationContext.ConnectionId));
        logger.LogInformation($"{invocationContext.ConnectionId} has connected, {invocationContext.UserId} {string.Join(",", invocationContext.Claims.Select(x => $"[{x.Key}, {x.Value}]"))}");
    }

    [FunctionName(nameof(OnDisconnected))]
    public void OnDisconnected([SignalRTrigger] InvocationContext invocationContext)
    {
    }
}
