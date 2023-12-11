
using System.Diagnostics;

namespace Pipeline.Services.Middlewares.Metrics;

public sealed class MetricsMiddleware : IPipelineMiddleware
{
    public async Task HandleAsync(FileProcessContext context, IPipelineStep step, PipelineDelegate inner)
    {
        var stepName = step.GetType().Name;

        var watch = Stopwatch.StartNew();
        try
        {
            context.ProcessData[$"Metrics.{stepName}.Started"] = DateTime.UtcNow;

            await inner(context);
        }
        finally
        {
            watch.Stop();

            context.ProcessData[$"Metrics.{stepName}.Completed"] = DateTime.UtcNow;
            context.ProcessData[$"Metrics.{stepName}.Elapsed"] = watch.Elapsed;
        }
    }
}
