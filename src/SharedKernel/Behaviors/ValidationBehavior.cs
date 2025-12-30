using FluentResults;
using FluentValidation;
using MediatR;

namespace TheShop.SharedKernel.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        return CreateValidationResult(failures);
    }

    private static TResponse CreateValidationResult(List<FluentValidation.Results.ValidationFailure> failures)
    {
        var errors = failures.Select(f => new Error(f.ErrorMessage)
            .WithMetadata("PropertyName", f.PropertyName))
            .ToList();

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var failMethod = typeof(Result)
                .GetMethods()
                .First(m => m.Name == "Fail" && m.IsGenericMethod && m.GetParameters().Length == 1 
                    && m.GetParameters()[0].ParameterType == typeof(IEnumerable<IError>))
                .MakeGenericMethod(resultType);

            return (TResponse)failMethod.Invoke(null, [errors.AsEnumerable<IError>()])!;
        }

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(errors);
        }

        throw new ValidationException(failures);
    }
}
