namespace Pipeline.Services;

public delegate Task PipelineDelegate(FileProcessContext context);

public interface IPipelineMiddleware
{
    Task HandleAsync(FileProcessContext context, IPipelineStep step, PipelineDelegate inner);
}
