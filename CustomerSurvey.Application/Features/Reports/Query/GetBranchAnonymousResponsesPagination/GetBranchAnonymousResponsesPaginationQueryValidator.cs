using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchAnonymousResponsesPagination;

internal sealed class GetBranchAnonymousResponsesPaginationQueryValidator
    : AbstractValidator<GetBranchAnonymousResponsesPaginationQuery>
{
    public GetBranchAnonymousResponsesPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.GetBranchAnonymousResponses_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.GetBranchAnonymousResponses_PageSize_Invalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ErrorMessage.GetBranchAnonymousResponses_PageSize_Max);

        RuleFor(x => x)
            .Must(x =>
                !x.From.HasValue ||
                !x.To.HasValue ||
                x.From.Value <= x.To.Value)
            .WithMessage(ErrorMessage.GetBranchAnonymousResponses_DateRange_Invalid);

        RuleFor(x => x.MinScorePercentage)
            .InclusiveBetween(0m, 100m)
            .When(x => x.MinScorePercentage.HasValue)
            .WithMessage(ErrorMessage.GetBranchAnonymousResponses_MinScore_Invalid);

        RuleFor(x => x.MaxScorePercentage)
            .InclusiveBetween(0m, 100m)
            .When(x => x.MaxScorePercentage.HasValue)
            .WithMessage(ErrorMessage.GetBranchAnonymousResponses_MaxScore_Invalid);

        RuleFor(x => x)
            .Must(x =>
                !x.MinScorePercentage.HasValue ||
                !x.MaxScorePercentage.HasValue ||
                x.MinScorePercentage.Value <= x.MaxScorePercentage.Value)
            .WithMessage(ErrorMessage.GetBranchAnonymousResponses_ScoreRange_Invalid);
    }
}
