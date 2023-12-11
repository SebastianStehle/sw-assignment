using System.Threading.Tasks.Dataflow;

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods

namespace Pipeline.Services;

public sealed class PipelineRunner : IHostedService
{
    private readonly ActionBlock<FileProcessContext> pipeline;
    private readonly IDataSource source;

    public PipelineRunner(
        IDataSource source,
        IEnumerable<IPipelineStep> pipelineSteps,
        IEnumerable<IPipelineMiddleware> pipelineMiddlewares)
    {
        var steps = pipelineSteps.Select(x => new MiddlewareStep(x, pipelineMiddlewares)).ToArray();

        pipeline = new ActionBlock<FileProcessContext>(async context =>
        {
            foreach (var step in steps)
            {
                await step.ProcessAsync(context);
            }
        },
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 10,
            MaxMessagesPerTask = 1,
            BoundedCapacity = 2
        });

        this.source = source;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() =>
        {
            source.Listen(context =>
            {
                pipeline.SendAsync(context).Wait();
            });
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
