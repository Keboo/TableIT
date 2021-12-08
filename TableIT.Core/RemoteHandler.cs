using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.SignalR.Management;
using System;
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
        private readonly HubConnection _connection;

        private readonly string _serverName;
        private readonly string _hubName;
        private readonly string _endpoint;
        private readonly string _accessKey;

        public RemoteHandler(string endpoint, string accessKey, string hubName)
        {


            _serverName = GenerateServerName();
            _hubName = hubName;
            _endpoint = endpoint;
            _accessKey = accessKey;

            var serviceManager = new ServiceManagerBuilder().WithOptions(option =>
            {
                option.ConnectionString = _connectionString;
                option.ServiceTransportType = _serviceTransportType;
            })
            //Uncomment the following line to get more logs
            //.WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .BuildServiceManager();

            _hubContext = await serviceManager.CreateHubContextAsync(HubName, default);

            var url = $"{endpoint}/{hubName}";

            _connection = new HubConnectionBuilder()
                .WithUrl(url, option =>
                {
                    option.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(ServiceUtils.GenerateAccessToken(accessKey, url, "test-user"));
                    };
                }).Build();
        }

        public async Task SendRequest<TMessage>(TMessage message)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                await _connection.StartAsync();
            }
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }

            string method = typeof(TMessage).Name.ToLowerInvariant();

            string argument = System.Text.Json.JsonSerializer.Serialize(message);
            await _connection.InvokeAsync(method, message);

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
