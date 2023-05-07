namespace TableIT.Shared;

public interface IImageService
{
    Uri GetImageUrl(string imageId, int? width = null, int? height = null);
}   
