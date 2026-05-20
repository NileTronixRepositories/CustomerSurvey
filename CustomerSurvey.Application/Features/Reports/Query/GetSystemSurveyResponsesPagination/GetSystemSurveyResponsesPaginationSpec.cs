using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;

internal sealed class GetSystemSurveyResponsesPaginationSpec
    : Specification<SurveyResponse, SystemSurveyResponsePaginationItemResponse>
{
    public GetSystemSurveyResponsesPaginationSpec(
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        GetSystemSurveyResponsesPaginationQuery query)
    {
        AddCriteria(x =>
            x.SubmittedOnUtc >= fromUtc &&
            x.SubmittedOnUtc < toExclusiveUtc);

        if (query.BranchId.HasValue)
        {
            AddCriteria(x => x.Template.BranchId == query.BranchId.Value);
        }

        if (query.DepartmentId.HasValue)
        {
            AddCriteria(x => x.Operator.DepartmentId == query.DepartmentId.Value);
        }

        if (query.TemplateId.HasValue)
        {
            AddCriteria(x => x.TemplateId == query.TemplateId.Value);
        }

        if (query.MinScorePercentage.HasValue)
        {
            AddCriteria(x =>
                x.MaxScore > 0 &&
                x.ScorePercentage >= query.MinScorePercentage.Value);
        }

        if (query.MaxScorePercentage.HasValue)
        {
            AddCriteria(x =>
                x.MaxScore > 0 &&
                x.ScorePercentage <= query.MaxScorePercentage.Value);
        }

        if (query.HasComplaint.HasValue)
        {
            if (query.HasComplaint.Value)
            {
                AddCriteria(x =>
                    x.Answers.Any(answer =>
                        answer.QuestionType == QuestionType.Complain));
            }
            else
            {
                AddCriteria(x =>
                    !x.Answers.Any(answer =>
                        answer.QuestionType == QuestionType.Complain));
            }
        }

        if (query.HasVoice.HasValue)
        {
            if (query.HasVoice.Value)
            {
                AddCriteria(x =>
                    x.Answers.Any(answer =>
                        answer.QuestionType == QuestionType.Voice));
            }
            else
            {
                AddCriteria(x =>
                    !x.Answers.Any(answer =>
                        answer.QuestionType == QuestionType.Voice));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var searchText = query.SearchText.Trim();

            AddCriteria(x =>
                x.Template.Branch.NameEn.Contains(searchText) ||
                (x.Template.Branch.NameAr != null && x.Template.Branch.NameAr.Contains(searchText)) ||
                x.Template.Branch.Code.Contains(searchText) ||

                x.Operator.Department.NameEn.Contains(searchText) ||
                (x.Operator.Department.NameAr != null && x.Operator.Department.NameAr.Contains(searchText)) ||

                x.Template.NameEn.Contains(searchText) ||
                (x.Template.NameAr != null && x.Template.NameAr.Contains(searchText)) ||

                x.Operator.ApplicationUser.NameEn.Contains(searchText) ||
                (x.Operator.ApplicationUser.NameAr != null && x.Operator.ApplicationUser.NameAr.Contains(searchText)) ||

                x.CustomInputValues.Any(value =>
                    value.NameSnapshot.Contains(searchText) ||
                    (value.StringValue != null && value.StringValue.Contains(searchText))) ||

                x.Answers.Any(answer =>
                    answer.QuestionType == QuestionType.Complain &&
                    answer.TextAnswer != null &&
                    answer.TextAnswer.Contains(searchText)));
        }

        if (query.OrderSort == OrderSort.Oldest)
        {
            AddOrderBy(x => x.SubmittedOnUtc);
        }
        else
        {
            AddOrderByDescending(x => x.SubmittedOnUtc);
        }

        EnableTotalCount();

        ApplyPaging(
            query.PageNumber,
            query.PageSize);

        Select(x => new SystemSurveyResponsePaginationItemResponse
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
            ScorePercentage = x.ScorePercentage,
            IsScored = x.MaxScore > 0,

            HasComplaint = x.Answers.Any(answer =>
                answer.QuestionType == QuestionType.Complain),

            HasVoice = x.Answers.Any(answer =>
                answer.QuestionType == QuestionType.Voice)
        });
    }
}