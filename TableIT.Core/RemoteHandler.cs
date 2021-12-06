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
        }

        public async Task SendRequest<TMessage>(TMessage message)
        {
            string content = System.Text.Json.JsonSerializer.Serialize(new PayloadMessage
            {
                Target = typeof(TMessage).Name.ToLowerInvariant(),
                Arguments = new[]
                {
                    System.Text.Json.JsonSerializer.Serialize(message)
                }
            });
            await SendRequest(content, _hubName);
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
            catch(Exception ex)
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
