using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace TableIT.Win;
public class Image
{
    public Image(StorageFile file, string id = null)
    {
        File = file;
        if (id is not null)
        {
            Id = id;
        }
    }

    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name => File.DisplayName;
    public string Version { get; }
    public StorageFile File { get; }

    internal async Task<ImageSource> GetImageSource()
    {
        BitmapImage bitmapImage = new();
        await bitmapImage.SetSourceAsync(await File.OpenReadAsync());
        return bitmapImage;
    }

    internal async Task<byte[]> GetBytes(int? widthRequest, int? heightRequest)
    {
        using var ras = await File.OpenReadAsync();
        using var stream = ras.AsStreamForRead();
        using var image = SKImage.FromEncodedData(stream);
        using var bitmap = SKBitmap.FromImage(image);

        if (widthRequest is not null && heightRequest is not null)
        {
            using var resized = bitmap.Resize(new SKImageInfo(heightRequest.Value, widthRequest.Value), SKFilterQuality.Medium);
            using SKData resizedData = resized.Encode(SKEncodedImageFormat.Jpeg, 100);
            return resizedData.ToArray();
        }
        else if (heightRequest is not null)
        {
            int thumbnailWidth = (int)(bitmap.Height / (double)bitmap.Width * heightRequest.Value);
            using var resized = bitmap.Resize(new SKImageInfo(heightRequest.Value, thumbnailWidth), SKFilterQuality.Medium);
            using SKData resizedData = resized.Encode(SKEncodedImageFormat.Jpeg, 100);
            return resizedData.ToArray();
        }
        else if (widthRequest is not null)
        {
            int thumbnailHeight = (int)(bitmap.Width / (double)bitmap.Height * widthRequest.Value);
            using var resized = bitmap.Resize(new SKImageInfo(thumbnailHeight, widthRequest.Value), SKFilterQuality.Medium);
            using SKData resizedData = resized.Encode(SKEncodedImageFormat.Jpeg, 100);
            return resizedData.ToArray();
        }
        else
        {
            using SKData resizedData = bitmap.Encode(SKEncodedImageFormat.Jpeg, 100);
            return resizedData.ToArray();
        }
    }
}
