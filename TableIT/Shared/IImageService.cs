using TableIT.Shared.Resources;

namespace TableIT.Shared;

public interface IImageService
{
    string GetImageUrl(string imageId, int? width = null, int? height = null);

    Task<IReadOnlyList<ImageResource>?> GetImageResourcesAsync();
}   
