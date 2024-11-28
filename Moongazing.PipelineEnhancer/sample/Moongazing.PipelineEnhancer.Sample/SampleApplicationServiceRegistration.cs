using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moongazing.PipelineEnhancer.Sample;

public static class SampleApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        PipelineEnhancer.BehaviorExtensions.AddPipelineBehaviors(services);
        return services;
    }
}


public class SampleProgramcs
{
        //    builder.Services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = redisConnectionString;
        //});
}
