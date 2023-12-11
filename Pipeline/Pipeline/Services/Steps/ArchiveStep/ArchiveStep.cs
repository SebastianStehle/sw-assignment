
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

        // Do not process this file any further.
        context.ProcessResult = FileProcessResult.Skipped;

        var archiveFolder = Path.Combine(context.WorkingFile.DirectoryName!, "archive");

        try
        {
            using (var fs = context.WorkingFile.OpenRead())
            {
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(archiveFolder);
                }
            }

            foreach (var file in Directory.GetFiles(archiveFolder))
            {
                var fileInfo = new FileInfo(file);

                // The conversion creates a self contained file out of that.
                if (!fileInfo.Name.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) &&
                    !fileInfo.Name.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var subContext = new FileProcessContext
                {
                    Stream = null!,
                    // We work on this file directly.
                    WorkingFile = fileInfo,
                };

                subContext.Metadata[MetadataKeys.FileSize] = fileInfo.Length;
                subContext.Metadata[MetadataKeys.FileName] = fileInfo.Name;

                await runner.ProcessInlineAsync(subContext);
            }
        }
        finally
        {
            Directory.Delete(archiveFolder, true);
        }
    }
}
