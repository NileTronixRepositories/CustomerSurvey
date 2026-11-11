using CustomerSurvey.Domain.Entities;
using FluentValidation;

namespace CustomerSurvey.Application.Features.ResponseFlags;

internal sealed class CreateResponseFlagValidator : AbstractValidator<CreateResponseFlagCommand>
{
    public CreateResponseFlagValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Brief).NotEmpty().MaximumLength(SurveyResponseFlag.MaxBriefLength);
        RuleFor(x => x.Answer).NotEmpty().MaximumLength(SurveyResponseFlag.MaxAnswerLength)
            .When(x => x.Answer is not null);
    }
}

internal sealed class UpdateResponseFlagBriefValidator : AbstractValidator<UpdateResponseFlagBriefCommand>
{
    public UpdateResponseFlagBriefValidator()
        => RuleFor(x => x.Brief).NotEmpty().MaximumLength(SurveyResponseFlag.MaxBriefLength);
}

internal sealed class WriteResponseFlagAnswerValidator : AbstractValidator<WriteResponseFlagAnswerCommand>
{
    public WriteResponseFlagAnswerValidator()
        => RuleFor(x => x.Answer).NotEmpty().MaximumLength(SurveyResponseFlag.MaxAnswerLength);
}

internal sealed class GetFlaggedResponsesValidator : AbstractValidator<GetFlaggedResponsesQuery>
{
    public GetFlaggedResponsesValidator()
    {
        RuleFor(x => x.PageNumber).InclusiveBetween(1, int.MaxValue / 100);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.OrderSort).IsInEnum();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.To).Must((query, to) => !to.HasValue ||
            (to.Value < DateOnly.MaxValue && (!query.From.HasValue || to.Value >= query.From.Value)));
    }
}
