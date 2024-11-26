
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Moongazing.PipelineEnhancer.Pipelines.Performance;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IIntervalRequest
{
    private readonly Stopwatch stopwatch;
    private readonly ILogger logger;

    public PerformanceBehavior(Stopwatch stopwatch, ILogger logger)
    {
        this.stopwatch = stopwatch;
        this.logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = request.GetType().Name;

        TResponse response;

        try
        {
            stopwatch.Start();
            response = await next();
        }
        finally
        {
            stopwatch.Stop();

            if (stopwatch.Elapsed.TotalSeconds > request.Interval)
            {
                string message = $"Performance -> {requestName} took {stopwatch.Elapsed.TotalSeconds} seconds";

                logger.Log(LogLevel.Information, message);
            }

            stopwatch.Reset();
        }

        return response;
    }
}
