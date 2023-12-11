namespace Pipeline.Services.Steps.Deletion;

public sealed class DeleteStep : IPipelineStep
{
    private readonly IDataSource source;

    public DeleteStep(IDataSource source)
    {
        this.source = source;
    }

    public async Task ProcessAsync(FileProcessContext context)
    {
        await source.DeleteAsync(context);
    }
}
