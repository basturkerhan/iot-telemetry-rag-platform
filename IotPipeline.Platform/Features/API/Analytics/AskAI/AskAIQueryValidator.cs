using FluentValidation;

namespace IotPipeline.Platform.Features.API.Analytics.AskAI;

public class AskAIQueryValidator
    : AbstractValidator<AskAIQuery>
{
    public AskAIQueryValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Please enter valid question")
            .MinimumLength(5).WithMessage("The question must be greater than 5 characters.");
    }
}
