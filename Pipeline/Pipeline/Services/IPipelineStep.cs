namespace Pipeline.Services;

public interface IPipelineStep
{
    Task ProcessAsync(FileProcessContext context);
}
