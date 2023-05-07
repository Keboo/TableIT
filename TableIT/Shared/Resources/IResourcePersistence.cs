namespace TableIT.Shared.Resources;

public interface IResourcePersistence
{
    Task<IReadOnlyList<ResourceData>> GetAll();
    Task Save(IEnumerable<ResourceData> data);
}
