#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace TableIT
{
    internal class ImageManager
    {
        private Image? Current { get; set; }
        private List<Image> Images { get; } = new();

        public ImageManager()
        {
        }

        public async Task Load()
        {
            Images.Clear();
            try
            {
                StorageFolder s1 = ApplicationData.Current.LocalFolder;
                var d1 = await s1.GetFoldersAsync();

                StorageFolder storageFolder = ApplicationData.Current.LocalCacheFolder;
                
                StorageFolder imagesFolder = await GetImagesFolder();
                StorageFolder embeddedFolder = await imagesFolder.CreateFolderAsync("Embedded", CreationCollisionOption.OpenIfExists);
                //Ensure bundled images exist
                foreach (var file in Directory.EnumerateFiles("Images"))
                {
                    var sourceFile = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(file));
                    StorageFile targetFile;
                    try
                    {
                        targetFile = await embeddedFolder.CreateFileAsync(Path.GetFileName(file), CreationCollisionOption.FailIfExists);
                    }
                    catch 
                    { 
                        continue;
                    }
                    await sourceFile.CopyAndReplaceAsync(targetFile);
                }
                await AddItemsFromFolder(embeddedFolder);
                await AddItemsFromFolder(imagesFolder);

                async Task AddItemsFromFolder(StorageFolder storageFolder)
                {
                    foreach (StorageFile file in await storageFolder.GetFilesAsync())
                    {
                        Images.Add(new Image(file));
                        //var thumbnail = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem);

                    }
                }
            }
            catch(Exception)
            {

            }
            
        }

        public Task<Image?> GetCurrentImage()
        {
            return Task.FromResult<Image?>(Current ??= Images.FirstOrDefault());
        }

        public Task<Image?> SetCurrentImage(Guid imageId)
        {
            if (Images.FirstOrDefault(x => x.Id == imageId) is { } image)
            {
                return Task.FromResult<Image?>(Current = image);
            }
            return Task.FromResult<Image?>(null);
        }

        public async IAsyncEnumerable<Image> GetImages()
        {
            await Task.Yield();
            foreach(var image in Images)
            {
                yield return image;
            }
        }

        private static async Task<StorageFolder> GetImagesFolder()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalCacheFolder;

            return await storageFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
        }

        public async Task AddImage(string name, byte[] data, Guid id)
        {
            StorageFolder folder = await GetImagesFolder();
            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
            using var writeStream = await file.OpenStreamForWriteAsync();
            await writeStream.WriteAsync(data, 0, data.Length);
            Images.Add(new Image(file, id));
        }
    }
}
