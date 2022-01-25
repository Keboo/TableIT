#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace TableIT;

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
            await AddItemsFromFolder(imagesFolder);

            async Task AddItemsFromFolder(StorageFolder storageFolder)
            {
                foreach (StorageFile file in await storageFolder.GetFilesAsync())
                {
                    if (file.Name.EndsWith("json")) continue;
                    Images.Add(new Image(file));
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

    public Task<Image?> SetCurrentImage(string imageId)
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

    public async Task AddImage(string name, byte[] data, string id)
    {
        StorageFolder folder = await GetImagesFolder();
        StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
        using var writeStream = await file.OpenStreamForWriteAsync();
        await writeStream.WriteAsync(data, 0, data.Length);
        Images.Add(new Image(file, id));
    }

    public bool HasImage(string id, string version) => Images.Any(x => x.Id == id && x.Version == version);

    private class ImageConfig
    {
        public string? Id { get; set; }
        public string? Version { get; set; }
    }
}
