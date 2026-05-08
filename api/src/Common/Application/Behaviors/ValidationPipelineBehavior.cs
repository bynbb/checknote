namespace Checknote.Common.Application.Behaviors;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Checknote.Common.Domain;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

public sealed class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly MethodInfo GenericFailureMethod = typeof(Result)
        .GetMethods()
        .Single(method =>
            method.Name == nameof(Result.Failure) &&
            method.IsGenericMethodDefinition &&
            method.GetParameters().Length == 1);

    private readonly IEnumerable<IValidator<TRequest>> validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        this.validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        IValidator<TRequest>[] requestValidators = validators.ToArray();
        if (requestValidators.Length == 0)
        {
            return await next(cancellationToken);
        }

        ValidationContext<TRequest> validationContext = new(request);

        ValidationFailure[] validationFailures = (await Task.WhenAll(
                requestValidators.Select(validator => validator.ValidateAsync(validationContext, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .DistinctBy(failure => new { failure.PropertyName, failure.ErrorCode, failure.ErrorMessage })
            .ToArray();

        if (validationFailures.Length == 0)
        {
            return await next(cancellationToken);
        }

        ValidationError validationError = CreateValidationError(validationFailures);

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(validationError);
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type valueType = typeof(TResponse).GetGenericArguments()[0];
            object? failure = GenericFailureMethod.MakeGenericMethod(valueType).Invoke(null, [validationError]);

            return (TResponse)failure!;
        }

        throw new ValidationException(validationFailures);
    }

    private static ValidationError CreateValidationError(IEnumerable<ValidationFailure> validationFailures) =>
        new(validationFailures
            .Select(failure => Error.Validation(failure.ErrorCode, failure.ErrorMessage))
            .ToArray());
}
