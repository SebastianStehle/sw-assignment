
using CliWrap;
using CliWrap.Buffered;

namespace Pipeline.Services.Steps.OptimizeGlb;

public sealed class OptimizeGlbStep : IPipelineStep
{
    public async Task ProcessAsync(FileProcessContext context)
    {
        if (!context.TryGetMetadata<string>(MetadataKeys.Extension, out var extension) || !(extension is "glb" or "gltf"))
        {
            return;
        }

        var workingFolder = context.WorkingFile.DirectoryName!;

        var targetName = $"{Guid.NewGuid()}.glb";
        var targetPath = Path.Combine(workingFolder, targetName);

        var result = await Cli.Wrap("gltf-transform")
            .WithValidation(CommandResultValidation.None)
            .WithWorkingDirectory(workingFolder)
            .WithArguments($"optimize {context.WorkingFile.Name} {targetName} --compress draco --texture-compress webp")
            .ExecuteBufferedAsync();

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to invoke compressor. Got status code {result.ExitCode}. Output: {result.StandardOutput}. Error: {result.StandardError}");
        }

        context.WorkingFile = new FileInfo(targetPath);
        context.ProcessData["CompressOutput"] = result.StandardOutput;
    }
}
