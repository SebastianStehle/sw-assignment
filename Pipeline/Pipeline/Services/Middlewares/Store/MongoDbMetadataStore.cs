using MongoDB.Bson;
using MongoDB.Driver;

namespace Pipeline.Services.Middlewares.Store;

public sealed class MongoDbMetadataStore : IMetadataStore, IPipelineMiddleware
{
    private readonly IMongoCollection<FileMetadata> collection;

    public MongoDbMetadataStore(IMongoDatabase database)
    {
        collection = database.GetCollection<FileMetadata>("files");
    }

    public async Task HandleAsync(FileProcessContext context, IPipelineStep step, PipelineDelegate inner)
    {
        await inner(context);

        var entity = new FileMetadata
        {
            Id = context.ProcessId,
            Metadata = context.Metadata,
            ProcessData = context.ProcessData,
            ProcessResult = context.ProcessResult
        };

        await collection.ReplaceOneAsync(x => x.Id == entity.Id, entity, new ReplaceOptions { IsUpsert = true });
    }

    public async Task<FileMetadata?> FindOneAsync(Guid id)
    {
        return await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<FileMetadata>> QueryAllAsync()
    {
        return await collection.Find(new BsonDocument()).ToListAsync();
    }
}
