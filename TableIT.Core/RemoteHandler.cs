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

        private readonly string _hubName;
        private readonly string _userId;
        private readonly string _endpoint;

        public RemoteHandler(string endpoint, string hubName, string userId)
        {
            _hubName = hubName;
            _userId = userId;
            _endpoint = endpoint;
        }

        private async Task<(string Url, string AccessKey)> GetSignalRConnection(string userId)
        {
            var response = await Client.PostAsync(_endpoint + $"/negotiate?user={userId}", new StringContent(""));
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (data?.TryGetValue("url", out string url) == true &&
                    data.TryGetValue("accessToken", out string accessKey))
                {
                    return (url, accessKey);
                }
            }
            return ("", "");
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
            await SendRequest(content, _hubName, _userId);
        }

        private async Task SendRequest(string content, string hubName, string userId)
        {
            try
            {
                var (endpoint, accessKey) = await GetSignalRConnection(userId);

                //string url = GetBroadcastUrl(hubName);
                string url = GetSendToUserUrl(endpoint, hubName, userId);

                if (!string.IsNullOrEmpty(url))
                {
                    var request = BuildRequest(url, accessKey, content);

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

        private string GetSendToUserUrl(string endpoint, string hubName, string userId)
        {
            return $"{GetBaseUrl(endpoint, hubName)}/users/{userId}";
        }

        //private string GetSendToGroupUrl(string hubName, string group)
        //{
        //    return $"{GetBaseUrl(hubName)}/groups/{group}";
        //}

        //private string GetBroadcastUrl(string hubName)
        //{
        //    return $"{GetBaseUrl(hubName)}";
        //}

        private static string GetBaseUrl(string endpoint, string hubName)
        {
            return $"{endpoint}/api/v1/hubs/{hubName.ToLower()}";
        }


        private HttpRequestMessage BuildRequest(string url, string accessToken, string content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GetUrl(url));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            return request;
        }
    }
}
