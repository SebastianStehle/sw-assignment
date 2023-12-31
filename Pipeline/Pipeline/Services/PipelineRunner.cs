﻿using System.Threading.Tasks.Dataflow;

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods

namespace Pipeline.Services;

public sealed class PipelineRunner : IHostedService, IPipelineRunner
{
    private readonly MiddlewareStep[] steps;
    private readonly ActionBlock<FileProcessContext> pipeline;
    private readonly IDataSource source;

    public PipelineRunner(
        IDataSource source,
        IEnumerable<IPipelineStep> pipelineSteps,
        IEnumerable<IPipelineMiddleware> pipelineMiddlewares)
    {
        steps = pipelineSteps.Select(x => new MiddlewareStep(x, pipelineMiddlewares)).ToArray();

        pipeline = new ActionBlock<FileProcessContext>(ProcessInlineAsync,
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 10,
            MaxMessagesPerTask = 1,
            BoundedCapacity = 2
        });

        this.source = source;
    }

    public async Task ProcessInlineAsync(FileProcessContext context)
    {
        foreach (var step in steps)
        {
            await step.ProcessAsync(context);
        }
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
