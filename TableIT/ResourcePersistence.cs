#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            IList<ResourceData> data = await GetAll();
            foreach (var item in data)
            {
                item.IsCurrent = item.Id == id;
            }
            await SaveData(data);
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
            StorageFile file = await imagesFolder.CreateFileAsync(MetadataFile, CreationCollisionOption.OpenIfExists);
            using Stream fileStream = await file.OpenStreamForReadAsync();
            using var sr = new StreamReader(fileStream);
            string foo = await sr.ReadToEndAsync();
            fileStream.Position = 0;
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

        List<ResourceData> existingImages = (await GetAll()).ToList();
        existingImages.Add(new ResourceData(id, ""));

        await SaveData(existingImages);
    }

    private async Task SaveData(IList<ResourceData> newData)
    {
        StorageFolder imagesFolder = await GetImagesFolder();
        try
        {
            StorageFile file = await imagesFolder.CreateFileAsync(MetadataFile, CreationCollisionOption.ReplaceExisting);
            using Stream fileStream = await file.OpenStreamForWriteAsync();
            JsonSerializer.Serialize(fileStream, newData.ToArray());
            await fileStream.FlushAsync();
        }
        catch (Exception)
        {
            //TODO: Exception handling
        }
    }

    

    private static async Task<StorageFolder> GetImagesFolder()
    {
        StorageFolder storageFolder = ApplicationData.Current.LocalCacheFolder;

        return await storageFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
    }
}
