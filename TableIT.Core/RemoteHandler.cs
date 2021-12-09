using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
//using Microsoft.Azure.SignalR.Management;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TableIT.Core
{
    public class RemoteHandler
    {
        private static readonly HttpClient Client = new();
        private HubConnection? _connection;

        private readonly string _serverName;
        private readonly string _hubName;
        private readonly string _endpoint;
        private readonly string _accessKey;
        //private readonly ServiceManager _serviceManager;
        //private ServiceHubContext _hubContext;

        public RemoteHandler(string endpoint, string accessKey, string hubName)
        {
            _serverName = GenerateServerName();
            _hubName = hubName;
            _endpoint = endpoint;
            _accessKey = accessKey;

            //_serviceManager = new ServiceManagerBuilder()
            //    .WithOptions(option =>
            //{
            //    option.ConnectionString = "https://tableit.azurewebsites.net/message";
            //    option.ServiceTransportType = ServiceTransportType.Transient;
            //})
            //Uncomment the following line to get more logs
            //.WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            //.BuildServiceManager();



            //_hubContext = await serviceManager.CreateHubContextAsync(HubName, default);

            var url = $"{endpoint}/{hubName}";

            
        }

        private async Task<HubConnection?> Connect()
        {
            if (_connection is { } connection) return connection;

            connection = new HubConnectionBuilder()
                            .WithUrl("https://tableit.azurewebsites.net/message?user=test-user", option =>
                            {
                                option.SkipNegotiation = false;
                            })
                            .ConfigureLogging(builder => builder.AddConsole())
                            .Build();
            await connection.StartAsync();
            if (connection.State == HubConnectionState.Connected)
            {
                return _connection = connection;
            }
            //var response = await Client.PostAsync("https://tableit.azurewebsites.net/message/negotiate?user=test-user", new StringContent(""));
            //if (response.IsSuccessStatusCode)
            //{
            //    string json = await response.Content.ReadAsStringAsync();
            //    var values = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            //    if (values?.TryGetValue("url", out string url) == true &&
            //        values.TryGetValue("accessToken", out string accessToken))
            //    {
            //        connection = new HubConnectionBuilder()
            //                .WithUrl(url, option =>
            //                {
            //                    option.AccessTokenProvider = () =>
            //                    {
            //                        return Task.FromResult(accessToken)!;
            //                    };
            //                    option.DefaultTransferFormat = Microsoft.AspNetCore.Connections.TransferFormat.Text;
            //                    option.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
            //                })
            //                .ConfigureLogging(builder => builder.AddConsole())
            //                .Build();
            //        await connection.StartAsync();
            //        if (connection.State == HubConnectionState.Connected)
            //        {
            //            return _connection = connection;
            //        }
            //    }
            //}

            return null;
        }

        public async Task SendRequest<TMessage>(TMessage message)
        {


            //_hubContext ??= await _serviceManager.CreateHubContextAsync("TestHub", default);


            //if (_connection.State != HubConnectionState.Connected)
            //{
            //    await _connection.StartAsync();
            //}
            //if (_connection.State != HubConnectionState.Connected)
            //{
            //    return;
            //}

            var connection = await Connect();
            if (connection is not null)
            {
                string method = typeof(TMessage).Name.ToLowerInvariant();

                string argument = System.Text.Json.JsonSerializer.Serialize(message);
                //await _hubContext.Clients.All.SendCoreAsync(method, new object[] { argument });
                try
                {
                    await connection.SendAsync(method, message);
                }
                catch (Exception ex)
                {

                }
            }
            //string content = System.Text.Json.JsonSerializer.Serialize(new PayloadMessage
            //{
            //    Target = typeof(TMessage).Name.ToLowerInvariant(),
            //    Arguments = new[]
            //    {
            //        System.Text.Json.JsonSerializer.Serialize(message)
            //    }
            //});
            //await SendRequest(content, _hubName);
        }

        private async Task SendRequest(string content, string hubName)
        {
            try
            {
                string url = GetBroadcastUrl(hubName);

                if (!string.IsNullOrEmpty(url))
                {
                    var request = BuildRequest(url, _accessKey, content);

                    // ResponseHeadersRead instructs SendAsync to return once headers are read
                    // rather than buffer the entire response. This gives a small perf boost.
                    // Note that it is important to dispose of the response when doing this to
                    // avoid leaving the connection open.
                    using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    if (response.StatusCode != HttpStatusCode.Accepted)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Sent error: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private Uri GetUrl(string baseUrl)
        {
            return new UriBuilder(baseUrl).Uri;
        }

        //private string GetSendToUserUrl(string hubName, string userId)
        //{
        //    return $"{GetBaseUrl(hubName)}/users/{userId}";
        //}

        //private string GetSendToGroupUrl(string hubName, string group)
        //{
        //    return $"{GetBaseUrl(hubName)}/groups/{group}";
        //}

        private string GetBroadcastUrl(string hubName)
        {
            return $"{GetBaseUrl(hubName)}";
        }

        private string GetBaseUrl(string hubName)
        {
            return $"{_endpoint}/api/v1/hubs/{hubName.ToLower()}";
        }

        private static string GenerateServerName()
            => $"{Environment.MachineName}_{Guid.NewGuid():N}";

        private HttpRequestMessage BuildRequest(string url, string accessKey, string content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GetUrl(url));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", ServiceUtils.GenerateAccessToken(accessKey, url, _serverName));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            return request;
        }
    }
}
