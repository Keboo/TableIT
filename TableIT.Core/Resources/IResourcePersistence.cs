using System.Collections.Generic;
using System.Threading.Tasks;

namespace TableIT.Core.Imaging;

public interface IResourcePersistence
{
    Task<IReadOnlyList<ResourceData>> GetAll();
    Task Save(IEnumerable<ResourceData> data);
}
