using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed class GetSystemDashboardSurveyResponsesSpec
    : Specification<SurveyResponse, SystemDashboardSurveyResponseDto>
{
    public GetSystemDashboardSurveyResponsesSpec(
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? branchId,
        Guid? departmentId)
    {
        AddCriteria(x =>
            x.SubmittedOnUtc >= fromUtc &&
            x.SubmittedOnUtc < toExclusiveUtc);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.Template.BranchId == branchId.Value);
        }

        if (departmentId.HasValue)
        {
            AddCriteria(x => x.Operator.DepartmentId == departmentId.Value);
        }

        AddOrderByDescending(x => x.SubmittedOnUtc);

        Select(x => new SystemDashboardSurveyResponseDto
        {
            SnapshotPayload = x.Snapshot == null ? null : x.Snapshot.Payload,
            Criteria = new CustomerSurvey.Application.Features.Reports.Shared.EvaluationCriteria
            {
                NeutralMinimum = x.Template == null || x.Template!.Branch == null || x.Template!.Branch!.EvaluationSettings == null ? 60m : x.Template!.Branch!.EvaluationSettings.NeutralMinimum,
                SatisfiedMinimum = x.Template == null || x.Template!.Branch == null || x.Template!.Branch!.EvaluationSettings == null ? 80m : x.Template!.Branch!.EvaluationSettings.SatisfiedMinimum,
                MediumRiskMinimum = x.Template == null || x.Template!.Branch == null || x.Template!.Branch!.EvaluationSettings == null ? 50m : x.Template!.Branch!.EvaluationSettings.MediumRiskMinimum,
                HealthyMinimum = x.Template == null || x.Template!.Branch == null || x.Template!.Branch!.EvaluationSettings == null ? 75m : x.Template!.Branch!.EvaluationSettings.HealthyMinimum,
            },
            SurveyResponseId = x.Id,
            TemplateId = x.TemplateId,
            TemplateNameEn = x.Template!.NameEn,
            TemplateNameAr = x.Template!.NameAr,

            BranchId = x.Template!.BranchId,
            BranchNameEn = x.Template!.Branch!.NameEn,
            BranchNameAr = x.Template!.Branch!.NameAr,

            OperatorId = x.OperatorId,
            OperatorNameEn = x.Operator.ApplicationUser.NameEn,
            OperatorNameAr = x.Operator.ApplicationUser.NameAr,

            DepartmentId = x.Operator.DepartmentId,

            SubmittedOnUtc = x.SubmittedOnUtc,
            ActualScore = x.ActualScore,
            MaxScore = x.MaxScore,
            ScorePercentage = x.ScorePercentage
        });
    }
}