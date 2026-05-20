using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;

internal sealed class GetSystemSurveyResponseDetailsBasicSpec
    : Specification<SurveyResponse, SystemSurveyResponseDetailsBasicDto>
{
    public GetSystemSurveyResponseDetailsBasicSpec(Guid surveyResponseId)
    {
        AddCriteria(x => x.Id == surveyResponseId);

        Select(x => new SystemSurveyResponseDetailsBasicDto
        {
            SurveyResponseId = x.Id,

            BranchId = x.Template.BranchId,
            BranchNameEn = x.Template.Branch.NameEn,
            BranchNameAr = x.Template.Branch.NameAr,
            BranchCode = x.Template.Branch.Code,

            DepartmentId = x.Operator.DepartmentId,
            DepartmentNameEn = x.Operator.Department.NameEn,
            DepartmentNameAr = x.Operator.Department.NameAr,

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