namespace Pipeline.Services;

public class MiddlewareStep : IPipelineStep
{
    private readonly PipelineDelegate step;
    private readonly IPipelineStep inner;

    public MiddlewareStep(IPipelineStep inner, IEnumerable<IPipelineMiddleware> pipelineMiddlewares)
    {
        var step = new PipelineDelegate(inner.ProcessAsync);

        foreach (var pipeline in pipelineMiddlewares)
        {
            var currentStep = step;

            step = context => pipeline.HandleAsync(context, inner, currentStep);
        }

        this.step = step;
        this.inner = inner;
    }

    public Task ProcessAsync(FileProcessContext context)
    {
        return step(context);
    }

    public override string ToString()
    {
        return inner.GetType().Name;
    }
}
