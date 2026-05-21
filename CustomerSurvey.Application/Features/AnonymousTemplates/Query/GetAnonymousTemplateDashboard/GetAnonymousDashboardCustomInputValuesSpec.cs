using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed class GetAnonymousDashboardCustomInputValuesSpec
    : Specification<AnonymousSurveyResponseCustomInputValue, AnonymousDashboardCustomInputValueDto>
{
    public GetAnonymousDashboardCustomInputValuesSpec(
        Guid branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? anonymousTemplateId)
    {
        AddCriteria(x =>
            x.AnonymousSurveyResponse.AnonymousTemplate.Scope == AnonymousTemplateScope.Branch &&
            x.AnonymousSurveyResponse.AnonymousTemplate.BranchId == branchId &&
            x.AnonymousSurveyResponse.SubmittedOnUtc >= fromUtc &&
            x.AnonymousSurveyResponse.SubmittedOnUtc < toExclusiveUtc);

        if (anonymousTemplateId.HasValue)
        {
            AddCriteria(x => x.AnonymousSurveyResponse.AnonymousTemplateId == anonymousTemplateId.Value);
        }

        Select(x => new AnonymousDashboardCustomInputValueDto
        {
            AnonymousSurveyResponseId = x.AnonymousSurveyResponseId,
            NameSnapshot = x.NameSnapshot,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue,
            ScorePercentage = x.AnonymousSurveyResponse.ScorePercentage,
            MaxScore = x.AnonymousSurveyResponse.MaxScore
        });
    }
}
