using TableIT.Shared.Resources;

namespace TableIT.Shared;

public interface IImageService
{
    string GetImageUrl(string resourceId, int? width = null, int? height = null);

    Task<Stream> GetImageAsync(string resourceId);

    Task<IReadOnlyList<ImageResource>?> GetImageResourcesAsync();
}   
