using FluentValidation;
using MediatR;
using Moongazing.PipelineEnhancer.Exceptions;
using ValidationException = Moongazing.PipelineEnhancer.Exceptions.ValidationException;

namespace Moongazing.PipelineEnhancer.Pipelines.Validation;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        this.validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        ValidationContext<object> context = new(request);
        var errors = validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .GroupBy(
                keySelector: p => p.PropertyName,
                resultSelector: (propertyName, errors) =>
                    new ValidationExceptionModel { Property = propertyName, Errors = errors.Select(e => e.ErrorMessage) }
            )
            .ToList();

        if (errors.Count != 0)
            throw new ValidationException(errors);

        TResponse response = await next();
        return response;
    }
}
