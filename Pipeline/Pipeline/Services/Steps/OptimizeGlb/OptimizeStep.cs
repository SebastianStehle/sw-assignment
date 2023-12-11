
using CliWrap;
using CliWrap.Buffered;

namespace Pipeline.Services.Steps.OptimizeGlb;

public sealed class OptimizeStep : IPipelineStep
{
    public async Task ProcessAsync(FileProcessContext context)
    {
        if (!context.TryGetMetadata<string>(MetadataKeys.Extension, out var extension) || extension != "glb")
        {
            return;
        }

        var workingFolder = Path.Combine(Path.GetTempPath(), "glb");

        Directory.CreateDirectory(workingFolder);

        var sourceName = $"{Guid.NewGuid()}.glb";
        var sourcePath = Path.Combine(workingFolder, sourceName);
        var targetName = $"{Guid.NewGuid()}.glb";
        var targetPath = Path.Combine(workingFolder, targetName);

        try
        {
            var originalLength = context.Stream.Length;

            using (var fs = new FileStream(sourcePath, FileMode.Create))
            {
                await context.Stream.CopyToAsync(fs);            }

            var result = await Cli.Wrap("gltf-transform")
                .WithWorkingDirectory(workingFolder)
                .WithArguments($"optimize {sourceName} {targetName} --compress draco --texture-compress webp")
                .ExecuteBufferedAsync();

            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException($"Failed to invoke compressor. Got status code {result.ExitCode}. Output: {result.StandardOutput}. Error: {result.StandardError}");
            }

            context.Stream.Dispose();
            context.Stream = new FileStream(targetPath, FileMode.Open);

            context.ProcessData["CompressOutput"] = result.StandardOutput;
            context.ProcessData["CompressionRate"] = string.Format("Value: {0:P2}.", (double)context.Stream.Length / originalLength);
        }
        finally
        {
            File.Delete(sourcePath);
        }
    }
}
