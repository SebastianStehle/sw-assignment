namespace Pipeline.Services.Steps.ExtractMetadata;

public sealed class ExtractMetadataStep : IPipelineStep
{
    private static readonly Dictionary<string, string> KnownExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["glb"] = "model/gltf-binary",
        ["gltf"] = "model/gltf-binary",
        ["zip"] = "application-x-zip"
    };
    private readonly ILogger<ExtractMetadataStep> logger;


    public ExtractMetadataStep(ILogger<ExtractMetadataStep> logger)
    {
        this.logger = logger;
    }

    public Task ProcessAsync(FileProcessContext context)
    {
        if (!context.TryGetMetadata<string>(MetadataKeys.FileName, out var fileName))
        {
            return Task.CompletedTask;
        }

        try
        {
            var extension = context.WorkingFile.Extension[1..];

            if (KnownExtensions.TryGetValue(extension, out var mimeType))
            {
                context.Metadata[MetadataKeys.Extension] = extension;
                context.Metadata[MetadataKeys.MimeType] = mimeType;
            }
            else
            {
                logger.LogInformation("Skipping step, unknown extension {extension} found.", extension);
            }
        }
        catch
        {
            logger.LogInformation("Skipping step, file {fileName} has no extension.", fileName);
        }

        return Task.CompletedTask;
    }
}
