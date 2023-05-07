using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TableIT.Shared.Resources;
using Windows.Storage;

namespace TableIT.Win;


internal class ResourcePersistence : IResourcePersistence
{
    private const string MetadataFile = "data.json";
    private SemaphoreSlim Lock { get; } = new(1, 1);
    private IReadOnlyList<ResourceData>? Cache { get; set; }

    public async Task<Stream?> Get(string id)
    {
        StorageFolder imagesFolder = await GetImagesFolder();
        try
        {
            IReadOnlyList<ResourceData> data = await GetAll();
            foreach (var item in data)
            {
                item.IsCurrent = item.Id == id;
            }
            await Save(data);
            StorageFile file = await imagesFolder.GetFileAsync(id);
            return await file.OpenStreamForReadAsync();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<ResourceData>> GetAll()
    {
        if (Cache is { } cache)
        {
            return cache;
        }

        try
        {
            await Lock.WaitAsync();
            StorageFolder imagesFolder = await GetImagesFolder();
            StorageFile file = await imagesFolder.CreateFileAsync(MetadataFile, CreationCollisionOption.OpenIfExists);
            using Stream fileStream = await file.OpenStreamForReadAsync();
            using var sr = new StreamReader(fileStream);
            string foo = await sr.ReadToEndAsync();
            fileStream.Position = 0;
            return Cache = JsonSerializer.Deserialize<ResourceData[]>(fileStream) ?? Array.Empty<ResourceData>();
        }
        catch (Exception)
        {
            //TODO: Exception handling 
            return Array.Empty<ResourceData>();
        }
        finally
        {
            Lock.Release();
        }
    }

    public async Task Save(string id, Stream data)
    {
        StorageFolder imagesFolder = await GetImagesFolder();
        StorageFile file = await imagesFolder.CreateFileAsync(id, CreationCollisionOption.ReplaceExisting);
        using var writeStream = await file.OpenStreamForWriteAsync();
        await data.CopyToAsync(writeStream);

        List<ResourceData> existingImages = (await GetAll()).ToList();
        existingImages.Add(new ResourceData(id, ""));

        await Save(existingImages);
    }

    public async Task Save(IEnumerable<ResourceData> newData)
    {
        try
        {
            await Lock.WaitAsync();
            StorageFolder imagesFolder = await GetImagesFolder();
            StorageFile file = await imagesFolder.CreateFileAsync(MetadataFile, CreationCollisionOption.ReplaceExisting);
            using Stream fileStream = await file.OpenStreamForWriteAsync();
            JsonSerializer.Serialize(fileStream, Cache = newData.ToArray());
            await fileStream.FlushAsync();
        }
        catch (Exception)
        {
            //TODO: Exception handling
        }
        finally
        {
            Lock.Release();
        }
    }

    private static async Task<StorageFolder> GetImagesFolder()
    {
        StorageFolder storageFolder = ApplicationData.Current.LocalCacheFolder;

        return await storageFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
    }
}
