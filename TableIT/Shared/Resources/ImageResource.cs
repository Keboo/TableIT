namespace TableIT.Shared.Resources;

public record ImageResource(string ResourceId, string DisplayName, string Version)
{
    public Uri GetImageUrl(int? width = null, int? height = null)
    {
        string relativeUrl = $"/image/{ResourceId}";
        string query = string.Join("&", GetQueryParamter());
        if (query.Length > 0)
        {
            relativeUrl += "?" + query;
        }
        return new Uri(relativeUrl, UriKind.Relative);

        IEnumerable<string> GetQueryParamter()
        {
            if (width is not null)
                yield return $"width={width}";
            if (height is not null)
                yield return $"height={height}";
        }
    }
}