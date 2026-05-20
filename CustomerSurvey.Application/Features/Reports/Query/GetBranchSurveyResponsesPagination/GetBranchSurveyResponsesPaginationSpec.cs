using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;

internal sealed class GetBranchSurveyResponsesPaginationSpec
    : Specification<SurveyResponse, BranchSurveyResponsePaginationItemResponse>
{
    public GetBranchSurveyResponsesPaginationSpec(
        Guid branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        GetBranchSurveyResponsesPaginationQuery query)
    {
        AddCriteria(x =>
            x.Template.BranchId == branchId &&
            x.SubmittedOnUtc >= fromUtc &&
            x.SubmittedOnUtc < toExclusiveUtc);

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

        Select(x => new BranchSurveyResponsePaginationItemResponse
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
            ScorePercentage = x.ScorePercentage,
            IsScored = x.MaxScore > 0,

            HasComplaint = x.Answers.Any(answer =>
                answer.QuestionType == QuestionType.Complain),

            HasVoice = x.Answers.Any(answer =>
                answer.QuestionType == QuestionType.Voice)
        });
    }
}