using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed class GetAnonymousTemplateDashboardQueryValidator
    : AbstractValidator<GetAnonymousTemplateDashboardQuery>
{
    public GetAnonymousTemplateDashboardQueryValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !x.From.HasValue ||
                !x.To.HasValue ||
                x.From.Value <= x.To.Value)
            .WithMessage(ErrorMessage.GetBranchDashboard_DateRange_Invalid);

        RuleFor(x => x.TopQuestionsCount)
            .InclusiveBetween(1, 20)
            .WithMessage(ErrorMessage.GetBranchDashboard_TopQuestionsCount_Invalid);

        RuleFor(x => x.CriticalResponsesCount)
            .InclusiveBetween(1, 50)
            .WithMessage(ErrorMessage.GetBranchDashboard_CriticalResponsesCount_Invalid);

        RuleFor(x => x.CriticalScoreThreshold)
            .InclusiveBetween(0m, 100m)
            .WithMessage(ErrorMessage.GetBranchDashboard_CriticalScoreThreshold_Invalid);

        RuleFor(x => x.GroupBy)
            .IsInEnum()
            .WithMessage(ErrorMessage.GetBranchDashboard_GroupBy_Invalid);
    }
}
