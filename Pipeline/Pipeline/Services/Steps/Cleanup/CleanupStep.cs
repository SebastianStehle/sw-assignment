namespace Pipeline.Services.Steps.Cleanup;

public sealed class CleanupStep : IPipelineStep
{
    public Task ProcessAsync(FileProcessContext context)
    {
        try
        {
            context.Stream.Dispose();
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}
