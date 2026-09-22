using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed class GetAnonymousDashboardSurveyResponsesSpec
    : Specification<AnonymousSurveyResponse, AnonymousDashboardSurveyResponseDto>
{
    public GetAnonymousDashboardSurveyResponsesSpec(
        Guid branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? anonymousTemplateId)
    {
        AddCriteria(x =>
            x.AnonymousTemplate.Scope == AnonymousTemplateScope.Branch &&
            x.AnonymousTemplate.BranchId == branchId &&
            x.SubmittedOnUtc >= fromUtc &&
            x.SubmittedOnUtc < toExclusiveUtc);

        if (anonymousTemplateId.HasValue)
        {
            AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId.Value);
        }

        AddOrderByDescending(x => x.SubmittedOnUtc);

        Select(x => new AnonymousDashboardSurveyResponseDto
        {
            AnonymousSurveyResponseId = x.Id,
            AnonymousTemplateId = x.AnonymousTemplateId,
            BranchId = x.AnonymousTemplate.BranchId!.Value,
            BranchNameEn = x.AnonymousTemplate.Branch!.NameEn,
            BranchNameAr = x.AnonymousTemplate.Branch!.NameAr,
            TemplateNameEn = x.AnonymousTemplate.NameEn,
            TemplateNameAr = x.AnonymousTemplate.NameAr,
            Scope = x.AnonymousTemplate.Scope,
            IsActive = x.AnonymousTemplate.IsActive,
            LogoPath = x.AnonymousTemplate.LogoPath,
            PublicUrl = x.AnonymousTemplate.PublicUrl,
            QrCode = x.AnonymousTemplate.QrCode,
            SubmittedOnUtc = x.SubmittedOnUtc,
            ActualScore = x.ActualScore,
            MaxScore = x.MaxScore,
            ScorePercentage = x.ScorePercentage
        });
    }
}
