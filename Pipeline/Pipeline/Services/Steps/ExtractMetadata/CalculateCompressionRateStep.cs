namespace Pipeline.Services.Steps.ExtractMetadata;

public class CalculateCompressionRateStep : IPipelineStep
{
    public Task ProcessAsync(FileProcessContext context)
    {
        if (!context.TryGetMetadata<int>(MetadataKeys.FileSize, out var originalFileSize))
        {
            return Task.CompletedTask;
        }

        var currentFileSize = context.WorkingFile.Length;

        if (currentFileSize >= originalFileSize)
        {
            return Task.CompletedTask;
        }

        context.Metadata[MetadataKeys.OriginalFileSize] = originalFileSize;
        context.ProcessData["CompressionRate"] = string.Format("Value: {0:P2}.", (double)currentFileSize / originalFileSize);
        return Task.CompletedTask;
    }
}
