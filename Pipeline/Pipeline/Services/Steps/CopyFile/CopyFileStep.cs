namespace Pipeline.Services.Steps.CopyFile;

public class CopyFileStep : IPipelineStep
{
    public async Task ProcessAsync(FileProcessContext context)
    {
        if (!context.TryGetMetadata<string>(MetadataKeys.RelativePath, out var relativePath))
        {
            return;
        }

        if (context.WorkingFile != null)
        {
            return;
        }

        var targetPath = Path.Combine(Path.GetTempPath(), "pipeline", context.ProcessId.ToString(), relativePath);
        var targetFile = new FileInfo(targetPath);

        Directory.CreateDirectory(targetFile.DirectoryName!);

        using (var fs = targetFile.OpenWrite())
        {
            await context.Stream.CopyToAsync(fs);
        }

        await context.Stream.DisposeAsync();

        context.WorkingFile = targetFile;
    }
}
