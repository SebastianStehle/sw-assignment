namespace Pipeline.Services;

public class MiddlewareStep : IPipelineStep
{
    private readonly PipelineDelegate step;

    public MiddlewareStep(IPipelineStep inner, IEnumerable<IPipelineMiddleware> pipelineMiddlewares)
    {
        var step = new PipelineDelegate(inner.ProcessAsync);

        foreach (var pipeline in pipelineMiddlewares)
        {
            var currentStep = step;

            step = context => pipeline.HandleAsync(context, inner, currentStep);
        }

        this.step = step;
    }

    public Task ProcessAsync(FileProcessContext context)
    {
        return step(context);
    }
}
