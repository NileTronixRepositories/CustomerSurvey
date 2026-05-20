using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed class GetBranchSurveyResponseDetailsBasicSpec
    : Specification<SurveyResponse, BranchSurveyResponseDetailsBasicDto>
{
    public GetBranchSurveyResponseDetailsBasicSpec(
        Guid surveyResponseId,
        Guid branchId)
    {
        AddCriteria(x =>
            x.Id == surveyResponseId &&
            x.Template.BranchId == branchId);

        Select(x => new BranchSurveyResponseDetailsBasicDto
        {
            SurveyResponseId = x.Id,
            TemplateId = x.TemplateId,
            TemplateNameEn = x.Template.NameEn,
            TemplateNameAr = x.Template.NameAr,
            OperatorId = x.OperatorId,
            OperatorNameEn = x.Operator.ApplicationUser.NameEn,
            OperatorNameAr = x.Operator.ApplicationUser.NameAr,
            SubmittedOnUtc = x.SubmittedOnUtc,
            ActualScore = x.ActualScore,
            MaxScore = x.MaxScore,
            ScorePercentage = x.ScorePercentage
        });
    }
}