using System.Net.Http.Json;
using TableIT.Shared.Resources;

namespace TableIT.Shared;

public class ImageService : IImageService
{
    public ImageService(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }

    public async Task<IReadOnlyList<ImageResource>?> GetImageResourcesAsync()
    {
        return await HttpClient.GetFromJsonAsync<ImageResource[]>("/api/image/list");
    }

    public Task<Stream> GetImageAsync(string resourceId)
    {
        return HttpClient.GetStreamAsync(GetImageUrl(resourceId));
    }

    public string GetImageUrl(string resourceId, int? width = null, int? height = null)
    {
        if (string.IsNullOrWhiteSpace(resourceId)) throw new ArgumentException("Resource id must be specified", nameof(resourceId));

        string relativeUrl = $"/api/image/{resourceId}";
        string query = string.Join("&", GetQueryParamter());
        if (query.Length > 0)
        {
            relativeUrl += "?" + query;
        }
        return relativeUrl;

        IEnumerable<string> GetQueryParamter()
        {
            if (width is not null)
                yield return $"width={width}";
            if (height is not null)
                yield return $"height={height}";
        }
    }
}
