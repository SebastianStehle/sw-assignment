namespace Pipeline.Services;

public record struct FileProcessResult(FileProcessStatus Status, string? Details = null)
{
    public static readonly FileProcessResult Pending =
        new(FileProcessStatus.Pending);

    public static readonly FileProcessResult Skipped =
        new(FileProcessStatus.Skipped);

    public static readonly FileProcessResult Success =
        new(FileProcessStatus.Success);

    public static FileProcessResult Failed(Exception exception) =>
        new(FileProcessStatus.Failed, exception.ToString());
}

public enum FileProcessStatus
{
    Pending,
    Failed,
    Skipped,
    Success
}