using FluentValidation;
using MassTransit;

namespace IotPipeline.Platform.Features.API.Shared.Behaviors;

public class ValidationFilter<T>(IEnumerable<IValidator<T>> validators) : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly IEnumerable<IValidator<T>> _validators = validators;

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        if (_validators.Any())
        {
            var validationContext = new ValidationContext<T>(context.Message);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(validationContext, context.CancellationToken))
            );

            var errors = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .Select(f => f.ErrorMessage)
                .ToList();

            if (errors.Count > 0)
            {
                throw new ValidationException(errors.Select(e => new FluentValidation.Results.ValidationFailure("", e)));
            }
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("validation");
    }
}
