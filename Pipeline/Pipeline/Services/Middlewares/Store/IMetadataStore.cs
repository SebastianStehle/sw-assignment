namespace Pipeline.Services.Middlewares.Store;

public interface IMetadataStore
{
    Task<IReadOnlyList<FileMetadata>> QueryAllAsync();

    Task<FileMetadata?> FindOneAsync(Guid id);
}
