namespace Pipeline.Services;

public sealed class FileProcessContext
{
    required public Stream Stream { get; set; }

    public Guid ProcessId { get; set; } = Guid.NewGuid();

    public Dictionary<string, object> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, object> ProcessData { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, object> TemporaryData { get; } = new(StringComparer.OrdinalIgnoreCase);

    public FileProcessResult ProcessResult { get; set; }

    public bool IsNewFile { get; set; }

    public bool TryGetMetadata<T>(string key, out T result)
    {
        return TryGetData(Metadata, key, out result);
    }

    public bool TryGetProcessData<T>(string key, out T result)
    {
        return TryGetData(ProcessData, key, out result);
    }

    public bool TryGetTemporaryData<T>(string key, out T result)
    {
        return TryGetData(TemporaryData, key, out result);
    }

    private static bool TryGetData<T>(Dictionary<string, object> source, string key, out T result)
    {
        if (source.TryGetValue(key, out var temp) && temp is T typed)
        {
            result = typed;
            return true;
        }

        result = default!;
        return false;
    }
}
