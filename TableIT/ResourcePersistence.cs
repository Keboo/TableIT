#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Imaging;
using Windows.Storage;

namespace TableIT;


internal class ResourcePersistence : IResourcePersistence
{
    private const string MetadataFile = "data.json";

    public async Task<Stream?> Get(string id)
    {
        StorageFolder imagesFolder = await GetImagesFolder();
        try
        {
            StorageFile file = await imagesFolder.GetFileAsync(id);
            return await file.OpenStreamForReadAsync();
        }
        catch(Exception)
        {
            return null;
        }
    }

    public async Task<IList<ResourceData>> GetAll()
    {
        StorageFolder imagesFolder = await GetImagesFolder();
        try
        {
            StorageFile file = await imagesFolder.GetFileAsync(MetadataFile);
            using Stream fileStream = await file.OpenStreamForReadAsync();
            return JsonSerializer.Deserialize<ResourceData[]>(fileStream) ?? Array.Empty<ResourceData>();
        }
        catch (Exception)
        {
            //TODO: Exception handling 
            return Array.Empty<ResourceData>();
        }
    }

    public async Task Save(string id, Stream data)
    {
        StorageFolder imagesFolder = await GetImagesFolder();
        StorageFile file = await imagesFolder.CreateFileAsync(id, CreationCollisionOption.ReplaceExisting);
        using var writeStream = await file.OpenStreamForWriteAsync();
        await data.CopyToAsync(writeStream);
    }

    private static async Task<StorageFolder> GetImagesFolder()
    {
        StorageFolder storageFolder = ApplicationData.Current.LocalCacheFolder;

        return await storageFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
    }
}
