namespace TableIT.Shared;

public class ImageService : IImageService
{
    public ImageService(Uri baseUrl)
    {
        BaseUrl = baseUrl;
    }

    public Uri BaseUrl { get; }

    public Uri GetImageUrl(string imageId, int? width = null, int? height = null)
    {
        if (string.IsNullOrWhiteSpace(imageId)) throw new ArgumentException("Image id must be specified", nameof(imageId));

        string relativeUrl = $"/image/{imageId}";
        string query = string.Join("&", GetQueryParamter());
        if (query.Length > 0)
        {
            relativeUrl += "?" + query;
        }
        return new Uri(BaseUrl, relativeUrl);

        IEnumerable<string> GetQueryParamter()
        {
            if (width is not null)
                yield return $"width={width}";
            if (height is not null)
                yield return $"height={height}";
        }
    }
}
