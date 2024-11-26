using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Moongazing.PipelineEnhancer.Pipelines.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>, ILoggableRequest
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger logger;

    public LoggingBehavior(IHttpContextAccessor httpContextAccessor, ILogger logger)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.Log(LogLevel.Information, request.GetType().Name, request);
        return await next();
    }
}
