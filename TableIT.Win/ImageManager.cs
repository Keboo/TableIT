using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Imaging;

namespace TableIT.Win;

internal class ImageManager
{
    private TableClient Client { get; }
    public IResourcePersistence Persistence { get; }

    public ImageManager(TableClient client, IResourcePersistence persistence)
    {
        Client = client;
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

    public async Task<Stream?> GetImage(string id, string version)
    {
        if (await Client.GetImage(id, version) is { } current)
        {
            IReadOnlyList<ResourceData> data = await Persistence.GetAll();
            Dictionary<string, ResourceData> keyed = new();

            foreach (var item in data)
            {
                item.IsCurrent = false;
                keyed[item.Id] = item;
            }
            if (!keyed.TryGetValue(id, out var resourceData))
            {
                resourceData = new ResourceData(id, version);
                keyed[id] = resourceData;
            }
            resourceData.IsCurrent = true;
            await Persistence.Save(keyed.Values);
            return current;
        }
        return null;
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
