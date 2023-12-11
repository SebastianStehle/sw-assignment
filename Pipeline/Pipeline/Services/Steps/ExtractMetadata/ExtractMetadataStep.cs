
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
        if (context.TryGetMetadata<string>(MetadataKeys.FileName, out var fileName))
        {
            var lastDot = fileName.LastIndexOf('.');
            if (lastDot >= 0)
            {
                var extension = fileName[(lastDot + 1)..];

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
            else
            {
                logger.LogInformation("Skipping step, file {fileName} has no extension.", fileName);
            }
        }
        else
        {
            logger.LogInformation("Skipping step, no file name found.");
        }

        return Task.CompletedTask;
    }
}
