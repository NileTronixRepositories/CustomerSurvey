using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed class GetDepartmentOperatorSurveyResponseDetailsBasicSpec
    : Specification<SurveyResponse, DepartmentOperatorSurveyResponseDetailsBasicDto>
{
    public GetDepartmentOperatorSurveyResponseDetailsBasicSpec(
        Guid surveyResponseId,
        Guid operatorId,
        Guid departmentId)
    {
        AddCriteria(x =>
            x.Id == surveyResponseId &&
            x.OperatorId == operatorId &&
            x.Operator.DepartmentId == departmentId);

        Select(x => new DepartmentOperatorSurveyResponseDetailsBasicDto
        {
            SurveyResponseId = x.Id,

            BranchId = x.Template.BranchId,
            BranchNameEn = x.Template.Branch.NameEn,
            BranchNameAr = x.Template.Branch.NameAr,
            BranchCode = x.Template.Branch.Code,

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
