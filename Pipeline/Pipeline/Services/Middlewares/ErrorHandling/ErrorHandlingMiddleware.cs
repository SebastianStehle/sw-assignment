
namespace Pipeline.Services.Middlewares.ErrorHandling;

public sealed class ErrorHandlingMiddleware : IPipelineMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        this.logger = logger;
    }

    public async Task HandleAsync(FileProcessContext context, IPipelineStep step, PipelineDelegate inner)
    {
        var stepName = step.GetType().Name; 

        logger.LogInformation("Step {stepName} started for file {fileId}", stepName, context.ProcessId);
        try
        {
            await inner(context);
            logger.LogTrace("Step {stepName} completed for file {fileId}", stepName, context.ProcessId);
        }
        catch (Exception ex)
        {
            logger.LogTrace("Step {stepName} failed for file {fileId}", stepName, context.ProcessId);

            context.ProcessResult = FileProcessResult.Failed(ex);
        }
    }
}