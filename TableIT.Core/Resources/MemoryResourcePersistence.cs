using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TableIT.Core.Imaging
{
    public class MemoryResourcePersistence : IResourcePersistence
    {
        public Dictionary<string, Stream> Data { get; } = new();

        public Task<Stream?> Get(string id)
        {
            if (Data.TryGetValue(id, out Stream? stream))
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }
                return Task.FromResult<Stream?>(stream);
            }
            return Task.FromResult<Stream?>(null);
        }

        public Task<IList<ResourceData>> GetAll()
        {
            return Task.FromResult<IList<ResourceData>>(
                Data.Select(kvp => new ResourceData(kvp.Key, "1")).ToArray());
        }

        public Task Save(string id, Stream data)
        {
            Data[id] = data;
            return Task.CompletedTask;
        }
    }
}
