
using System.IO.Compression;

namespace Pipeline.Services.Steps.ArchiveStep;

public sealed class ArchiveStep : IPipelineStep
{
    private readonly IServiceProvider serviceProvider;
    private IPipelineRunner? runner;

    public ArchiveStep(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task ProcessAsync(FileProcessContext context)
    {
        runner ??= serviceProvider.GetRequiredService<IPipelineRunner>();

        if (!context.TryGetMetadata<string>(MetadataKeys.Extension, out var extension) || extension != "zip")
        {
            return;
        }

        var workingFolder = Path.Combine(Path.GetTempPath(), "zip");

        Directory.CreateDirectory(workingFolder);

        var tempName = $"{Guid.NewGuid()}.zip";
        var tempPath = Path.Combine(workingFolder, tempName);

        var fs = new FileStream(tempPath, FileMode.Create);

        // The original stream is not writable, therefore we need a new temporary file.
        await context.Stream.CopyToAsync(fs);

        // Start the file from beginning.
        fs.Seek(0, SeekOrigin.Begin);

        var archive = new ZipArchive(fs, ZipArchiveMode.Update);

        foreach (var entry in archive.Entries)
        {
            var originalLength = entry.Length;
            var originalStream = entry.Open();

            var subContext = new FileProcessContext
            {
                Stream = originalStream
            };

            subContext.Metadata[MetadataKeys.FileSize] = originalLength;
            subContext.Metadata[MetadataKeys.FileName] = entry.Name;

            await runner.ProcessInlineAsync(subContext);

            if (subContext.ProcessResult.Status == FileProcessStatus.Failed)
            {
                throw new InvalidOperationException($"Failed to process {entry.Name} with error: {subContext.ProcessResult.Details}.");
            }

            if (subContext.Stream != originalStream)
            {
                entry.Delete();

                var updatedEntry = archive.CreateEntry(entry.Name);

                using (var newStream = updatedEntry.Open())
                {
                    await subContext.Stream.CopyToAsync(newStream);
                }
            }
            
            foreach (var (key, value) in subContext.ProcessData)
            {
                context.ProcessData[$"{entry.Name}__{key}"] = value;
            }
        }

        // Start the file from beginning for further calls.
        fs.Seek(0, SeekOrigin.Begin);

        context.Stream.Dispose();
        context.Stream = fs;
    }
}
