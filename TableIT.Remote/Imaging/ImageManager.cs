using Microsoft.Maui.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TableIT.Remote.Imaging
{
    public record RemoteImage(string ImageId, string Name, string? Version)
    {
        public ImageSource? Thumbnail { get; private set; }
        public byte[]? Image { get; private set; }

        internal async Task SetImageData(Stream data)
        {
            if (data is MemoryStream stream)
            {
                Image = stream.ToArray();
            }
            else
            {
                using var ms = new MemoryStream((int)data.Length);
                await data.CopyToAsync(ms);
                ms.Position = 0;
                Image = ms.ToArray();
            }
        }

        internal async Task SetThumbnailData(Stream data)
        {
            var ms = new MemoryStream((int)data.Length);
            await data.CopyToAsync(ms);
            ms.Position = 0;
            Thumbnail = ImageSource.FromStream(() => ms);
        }
    }

    public interface IImageManager
    {
        ValueTask<RemoteImage?> FindImage(string imageId);
        Task<IReadOnlyList<RemoteImage>> LoadImages(bool forceRefresh = false);
        Task<RemoteImage> LoadImage(RemoteImage image);
        Task<RemoteImage> LoadThumbnailImage(RemoteImage image);
    }


    public class ImageManager : IImageManager
    {
        private ConcurrentDictionary<string, RemoteImage> Images { get; } = new();

        public ImageManager(TableClientManager tableClientManager)
        {
            ClientManager = tableClientManager ?? throw new ArgumentNullException(nameof(tableClientManager));
        }

        public TableClientManager ClientManager { get; }

        public async ValueTask<RemoteImage?> FindImage(string imageId)
        {
            if (Images.TryGetValue(imageId, out RemoteImage? image))
            {
                return image;
            }
            await LoadImages(true);
            if (Images.TryGetValue(imageId, out image))
            {
                return image;
            }
            return null;
        }

        public async Task<RemoteImage> LoadImage(RemoteImage image)
        {
            if (Images.TryGetValue(image.ImageId, out RemoteImage? existing))
            {
                image = existing;
            }
            if (image.Image is null && ClientManager.GetClient() is { } client)
            {
                Stream? imageData = await client.GetImage(image.ImageId, image.Version);
                if (imageData is not null)
                {
                    await image.SetImageData(imageData);
                }
            }
            return image;
        }

        public async Task<RemoteImage> LoadThumbnailImage(RemoteImage image)
        {
            if (Images.TryGetValue(image.ImageId, out RemoteImage? existing))
            {
                image = existing;
            }
            if (image.Thumbnail is null && ClientManager.GetClient() is { } client)
            {
                Stream? imageData = await client.GetImage(image.ImageId, image.Version, width: 50);
                if (imageData is not null)
                {
                    await image.SetThumbnailData(imageData);
                }
            }
            return image;
        }

        public async Task<IReadOnlyList<RemoteImage>> LoadImages(bool forceRefresh = false)
        {
            if (Images.Count <= 0 || forceRefresh)
            {
                if (ClientManager.GetClient() is { } client)
                {
                    var images = await client.GetImages();
                    Images.Clear();
                    foreach (var image in images.Where(x => !string.IsNullOrEmpty(x.Id)))
                    {
                        Images[image.Id!] = new RemoteImage(image.Id!, image.Name ?? "", image.Version);
                    }
                }
            }
            return Images.Values.ToList();
        }
    }
}
