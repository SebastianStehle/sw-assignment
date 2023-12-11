using Microsoft.Extensions.Options;

namespace Pipeline.Services.Sources.FileSystem;

public class FileSystemDataSource : IDataSource
{
    private readonly FileSystemOptions options;
    private readonly ILogger<FileSystemDataSource> logger;

    public FileSystemDataSource(IOptions<FileSystemOptions> options, ILogger<FileSystemDataSource> logger)
    {
        this.options = options.Value;
        this.logger = logger;
    }

    public Task DeleteAsync(FileProcessContext context)
    {
        if (!options.DeleteFiles)
        {
            return Task.CompletedTask;
        }

        if (context.TryGetTemporaryData<string>("fullPath", out var fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete file {fullPath}.", fullPath);
            }
        }

        return Task.CompletedTask;
    }

    public void Listen(Action<FileProcessContext> listener)
    {
        var watcher = new FileSystemWatcher(options.SourceFolder, "*.*");

        watcher.Created += (sender, e) =>
        {
            var fileContext = CreateContext(e.FullPath);

            if (fileContext == null)
            {
                return;
            }

            fileContext.IsNewFile = true;

            listener(fileContext);
        };

        watcher.EnableRaisingEvents = true;

        foreach (var file in Directory.GetFiles(options.SourceFolder, "*.*", SearchOption.AllDirectories))
        {
            var fileContext = CreateContext(file);

            if (fileContext == null)
            {
                continue;
            }

            listener(fileContext);
        }
    }

    private FileProcessContext? CreateContext(string fullPath)
    {
        if (!options.FileExtensions.Any(e => fullPath.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var fileInfo = new FileInfo(fullPath);

        var context = new FileProcessContext
        {
            Stream = fileInfo.OpenRead()
        };

        context.Metadata[MetadataKeys.RelativePath] = GetRelativePath(fullPath);
        context.Metadata[MetadataKeys.FileName] = fileInfo.Name;
        context.Metadata[MetadataKeys.OriginalFileSize] = fileInfo.Length;
        context.Metadata[MetadataKeys.CreationTimeUtc] = fileInfo.CreationTimeUtc;

        context.TemporaryData["fullPath"] = fullPath;

        return context;
    }

    private string GetRelativePath(string fullePath)
    {
        return fullePath[options.SourceFolder.Length..];
    }
}
