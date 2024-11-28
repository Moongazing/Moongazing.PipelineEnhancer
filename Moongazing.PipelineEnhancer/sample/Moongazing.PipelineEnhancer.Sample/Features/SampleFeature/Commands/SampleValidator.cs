using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moongazing.PipelineEnhancer.Sample.Features.SampleFeature.Commands;

public class SampleValidator:AbstractValidator<CreateSampleCommand>
{
	public SampleValidator()
	{
		RuleFor(x => x.Name).MaximumLength(3);
	}
}
