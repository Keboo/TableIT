using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace TableIT
{
    public class Image
    {
        public Image(StorageFile file)
        {
            File = file;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string Name => File.DisplayName;
        public StorageFile File { get; }

        internal async Task<ImageSource> GetImageSource()
        {
            BitmapImage bitmapImage = new();
            await bitmapImage.SetSourceAsync(await File.OpenReadAsync());
            return bitmapImage;
        }

        internal async Task<byte[]> GetBytes()
        {
            using var ras = await File.OpenReadAsync();
            using var stream = ras.AsStreamForRead();
            using var image = SKImage.FromEncodedData(stream);
            using var bitmap = SKBitmap.FromImage(image);

            const int thumbnailHeight = 100;
            int thumbnailWidth = (int)((bitmap.Height / (double)bitmap.Width) * thumbnailHeight);
            using var resized = bitmap.Resize(new SKImageInfo(thumbnailHeight, thumbnailWidth), SKFilterQuality.Medium);
            using SKData resizedData = resized.Encode(SKEncodedImageFormat.Jpeg, 100);
            return resizedData.ToArray();
        }
    }
}
