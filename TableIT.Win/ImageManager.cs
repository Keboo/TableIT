using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableIT.Shared;
using TableIT.Shared.Resources;

namespace TableIT.Win;

internal class ImageManager
{
    private IImageService ImageService { get; }
    public IResourcePersistence Persistence { get; }

    public ImageManager(IImageService imageService, IResourcePersistence persistence)
    {
        ImageService = imageService;
        Persistence = persistence;
    }

    public async Task<Stream?> Load()
    {
        IReadOnlyList<ResourceData> data = await Persistence.GetAll();
        if (data.FirstOrDefault(x => x.IsCurrent) is { } current)
        {
            return await GetImage(current.Id, current.Version);
        }
        return null;
    }

    public async Task<Stream> GetImage(string id, string? version)
    {
        IReadOnlyList<ResourceData> data = await Persistence.GetAll();
        Dictionary<string, ResourceData> keyed = new();

        ResourceData? resourceData = null;
        foreach (var item in data)
        {
            item.IsCurrent = false;
            if (item.Id == id)
            {
                resourceData = item;

            }
            keyed[item.Id] = item;
        }
        if (resourceData is null)
        {
            resourceData = new(id, version);
            keyed[id] = resourceData;
        }
        resourceData.IsCurrent = true;
        await Persistence.Save(keyed.Values);
        
        //NB: Must copy the stream because to load the bitmap it must support seeking
        using Stream stream = await ImageService.GetImageAsync(id);
        MemoryStream ms = new();
        await stream.CopyToAsync(ms);
        ms.Position = 0;
        return ms;
    }

    public async Task UpdateCurrentImageData(double horizontalOffset, double verticalOffset, float zoomFactor)
    {
        IReadOnlyList<ResourceData> data = await Persistence.GetAll();
        if (data.FirstOrDefault(x => x.IsCurrent) is { } current)
        {
            current.HorizontalOffset = horizontalOffset;
            current.VerticalOffset = verticalOffset;
            current.ZoomFactor = zoomFactor;
            await Persistence.Save(data);
        }
    }

    public async Task<ResourceData?> GetResourceData(string id)
    {
        IReadOnlyList<ResourceData> data = await Persistence.GetAll();
        return data.FirstOrDefault(x => x.Id == id);
    }

    public async Task<ResourceData?> GetCurrentData()
    {
        IReadOnlyList<ResourceData> data = await Persistence.GetAll();
        return data.FirstOrDefault(x => x.IsCurrent);
    }
}
