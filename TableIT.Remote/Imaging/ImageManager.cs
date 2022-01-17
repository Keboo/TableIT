using Microsoft.Maui.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableIT.Core;

namespace TableIT.Remote.Imaging
{
    public record RemoteImage(Guid ImageId, string Name)
    {
        public ImageSource? Thumbnail { get; private set; }
        public ImageSource? Image { get; private set; }

        internal void SetImageData(byte[] data)
        {
            var ms = new MemoryStream(data);
            Image = ImageSource.FromStream(() => ms);
        }

        internal void SetThumbnailData(byte[] data)
        {
            var ms = new MemoryStream(data);
            Thumbnail = ImageSource.FromStream(() => ms);
        }
    }

    public interface IImageManager
    {
        ValueTask<RemoteImage?> FindImage(Guid imageId);
        Task<IReadOnlyList<RemoteImage>> LoadImages(bool forceRefresh = false);
        Task<RemoteImage> LoadImage(RemoteImage image);
        Task<RemoteImage> LoadThumbnailImage(RemoteImage image);
    }


    public class ImageManager : IImageManager
    {
        private ConcurrentDictionary<Guid, RemoteImage> Images { get; } = new();

        public ImageManager(TableClientManager tableClientManager)
        {
            ClientManager = tableClientManager ?? throw new ArgumentNullException(nameof(tableClientManager));
        }

        public TableClientManager ClientManager { get; }

        public async ValueTask<RemoteImage?> FindImage(Guid imageId)
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
                byte[] data = await client.GetImage(image.ImageId);
                image.SetImageData(data);
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
                byte[] data = await client.GetImage(image.ImageId, width:50);
                image.SetThumbnailData(data);
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
                    foreach (var image in images)
                    {
                        Images[image.Id] = new RemoteImage(image.Id, image.Name ?? "");
                    }
                }
            }
            return Images.Values.ToList();
        }
    }
}
