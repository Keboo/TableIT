using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TableIT.Core.Imaging
{
    public class MemoryResourcePersistence : IResourcePersistence
    {
        public Dictionary<string, (ResourceData, Stream?)> Data { get; } = new();

        public Task<Stream?> Get(string id)
        {
            if (Data.TryGetValue(id, out (ResourceData, Stream?) tuple) &&
                tuple.Item2 is not null)
            {
                var (_, stream) = tuple;
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                return Task.FromResult<Stream?>(stream);
            }
            return Task.FromResult<Stream?>(null);
        }

        public Task Save(IEnumerable<ResourceData> data)
        {
            foreach(var item in data)
            {
                if (Data.TryGetValue(item.Id, out (ResourceData, Stream?) tuple))
                {
                    tuple.Item1 = item;
                    Data[item.Id] = tuple;
                }
                else
                {
                    Data[item.Id] = (item, null);
                }
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ResourceData>> GetAll()
        {
            return Task.FromResult<IReadOnlyList<ResourceData>>(
                Data.Select(kvp => new ResourceData(kvp.Key, "memory")).ToArray());
        }

        public Task Save(string id, Stream data)
        {
            if (Data.TryGetValue(id, out (ResourceData, Stream?) tuple))
            {
                tuple.Item2 = data;
                Data[id] = tuple;
            }
            else
            {
                Data[id] = (new ResourceData(id, "memory"), data);
            }

            return Task.CompletedTask;
        }
    }
}
