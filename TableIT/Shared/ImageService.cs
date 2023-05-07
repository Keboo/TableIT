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

    public string GetImageUrl(string imageId, int? width = null, int? height = null)
    {
        if (string.IsNullOrWhiteSpace(imageId)) throw new ArgumentException("Image id must be specified", nameof(imageId));

        string relativeUrl = $"/api/image/{imageId}";
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
