namespace Pipeline.Services.Steps.Cleanup;

public sealed class CleanupStep : IPipelineStep
{
    public Task ProcessAsync(FileProcessContext context)
    {
        context.Stream.Dispose();
        return Task.CompletedTask;
    }
}
