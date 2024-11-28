using Doing.Retail.Core.Application.Pipelines.Authorization;
using MediatR;
using Moongazing.PipelineEnhancer.Pipelines.Caching;
using Moongazing.PipelineEnhancer.Pipelines.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moongazing.PipelineEnhancer.Sample.Features.SampleFeature.Queries;

public class GetSampleByIdQuery : IRequest<GetSampleByIdResponse>, ILoggableRequest, ISecuredRequest, ICachableRequest
{
    public int Id { get; set; }
    public string CacheKey => $"{GetType().Name}({Id})";
    public bool BypassCache { get; }
    public string? CacheGroupKey => "Sample";
    public TimeSpan? SlidingExpiration { get; }
    public string[] Roles => ["Admin"];
}


//Handler..
public class GetSampleByIdResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
}
