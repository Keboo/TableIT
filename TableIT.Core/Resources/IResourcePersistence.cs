using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TableIT.Core.Imaging
{
    public interface IResourcePersistence
    {
        Task Save(string id, Stream data);
        Task<Stream?> Get(string id);
        Task<IList<ResourceData>> GetAll();
    }
}
