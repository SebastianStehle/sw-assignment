namespace Pipeline.Services;

public interface IPipelineRunner
{
    Task ProcessInlineAsync(FileProcessContext context);
}
