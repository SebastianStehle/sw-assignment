namespace Pipeline.Services.Middlewares.Store;

public sealed class FileMetadata
{
    required public Guid Id { get; init; }

    required public Dictionary<string, object> Metadata { get; init; }

    required public Dictionary<string, object> ProcessData { get; init; }

    required public FileProcessResult ProcessResult { get; init; }
}
