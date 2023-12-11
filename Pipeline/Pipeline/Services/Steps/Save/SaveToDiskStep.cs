
using Microsoft.Extensions.Options;

namespace Pipeline.Services.Steps.Save;

public class SaveToDiskStep : IPipelineStep
{
    private readonly SaveToDiskOptions options;

    public SaveToDiskStep(IOptions<SaveToDiskOptions> options)
    {
        this.options = options.Value;
    }

    public async Task ProcessAsync(FileProcessContext context)
    {
        if (context.ProcessResult.Status == FileProcessStatus.Failed)
        {
            return;
        }

        if (!context.TryGetMetadata<string>(MetadataKeys.FileName, out var fileName))
        {
            fileName = "unknown.blob";
        }

        var fullName = $"{context.ProcessId}_{fileName}";
        var fullPath = Path.Combine(options.TargetFolder, fullName);

        using (var fs = new FileStream(fullPath, FileMode.Create))
        {
            await context.Stream.CopyToAsync(fs);
        }

        context.ProcessData["StorageName"] = fullName;
        context.ProcessData["StoragePath"] = fullPath;
    }
}
