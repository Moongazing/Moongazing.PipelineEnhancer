using Doing.Retail.Core.Application.Pipelines.Authorization;
using MediatR;
using Moongazing.PipelineEnhancer.Pipelines.Caching;
using Moongazing.PipelineEnhancer.Pipelines.Logging;
using Moongazing.PipelineEnhancer.Pipelines.Performance;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moongazing.PipelineEnhancer.Sample.Features.SampleFeature.Commands;

public class CreateSampleCommand : IRequest<CreatedSampleResponse>,
    ILoggableRequest, ISecuredRequest, ICacheRemoverRequest, IIntervalRequest
{
    public string Name { get; set; } // request



    public string[] Roles => ["Admin", "Write"]; // Can be constants
    public bool BypassCache { get; }
    public string? CacheGroupKey => "Sample"; // Can be constants
    public string? CacheKey => null;
    public int Interval => 15; // you can give a constant value

}


public class CreatedSampleResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
}

//Handler