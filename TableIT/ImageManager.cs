#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Imaging;

namespace TableIT;

internal class ImageManager
{
    private Image? Current { get; set; }
    private List<Image> Images { get; } = new();
    private TableClient Client { get; }
    public IResourcePersistence Persistence { get; }

    public ImageManager(TableClient client, IResourcePersistence persistence)
    {
        Client = client;
        Persistence = persistence;
    }

    public async Task<Stream?> Load()
    {
        Images.Clear();
        IList<ResourceData> data = await Persistence.GetAll();
        if (data.FirstOrDefault(x => x.IsCurrent) is { } current)
        {
            return await Client.GetImage(current.Id, current.Version);
        }
        return null;
        //TODO: Load last image if there was one

        //try
        //{
        //    StorageFolder s1 = ApplicationData.Current.LocalFolder;
        //    var d1 = await s1.GetFoldersAsync();

        //    StorageFolder storageFolder = ApplicationData.Current.LocalCacheFolder;
            
        //    StorageFolder imagesFolder = await GetImagesFolder();
        //    await AddItemsFromFolder(imagesFolder);

        //    async Task AddItemsFromFolder(StorageFolder storageFolder)
        //    {
        //        foreach (StorageFile file in await storageFolder.GetFilesAsync())
        //        {
        //            if (file.Name.EndsWith("json")) continue;
        //            Images.Add(new Image(file));
        //        }
        //    }
        //}
        //catch(Exception)
        //{

        //}
        
    }

    public async Task<Stream?> GetImage(string id, string version)
    {
        Stream? stream = await Client.GetImage(id, version);
        //TODO: Remember last image
        return stream;
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


    private class ImageConfig
    {
        public string? Id { get; set; }
        public string? Version { get; set; }
    }
}
