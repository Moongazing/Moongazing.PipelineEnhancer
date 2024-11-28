using Doing.Retail.Core.Application.Pipelines.Authorization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moongazing.PipelineEnhancer.Pipelines.Caching;
using Moongazing.PipelineEnhancer.Pipelines.Logging;
using Moongazing.PipelineEnhancer.Pipelines.Transaction;
using Moongazing.PipelineEnhancer.Pipelines.Validation;
using System.Reflection;

namespace Moongazing.PipelineEnhancer;

public static class BehaviorExtensions
{
    public static IServiceCollection AddPipelineBehaviors(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            configuration.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            configuration.AddOpenBehavior(typeof(CachingBehavior<,>));
            configuration.AddOpenBehavior(typeof(CacheRemovingBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(RequestValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionScopeBehavior<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }

}
