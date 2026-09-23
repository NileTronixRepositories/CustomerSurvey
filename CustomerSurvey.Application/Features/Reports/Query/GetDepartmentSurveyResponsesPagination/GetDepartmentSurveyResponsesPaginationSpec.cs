using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination;

internal sealed class GetDepartmentSurveyResponsesPaginationSpec
    : Specification<SurveyResponse, DepartmentSurveyResponsePaginationItemResponse>
{
    public GetDepartmentSurveyResponsesPaginationSpec(
        Guid departmentId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        GetDepartmentSurveyResponsesPaginationQuery query)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.Operator.DepartmentId == departmentId &&
            x.SubmittedOnUtc >= fromUtc &&
            x.SubmittedOnUtc < toExclusiveUtc);

        if (query.TemplateId.HasValue)
        {
            AddCriteria(x => x.TemplateId == query.TemplateId.Value);
        }

        if (query.MinScorePercentage.HasValue)
        {
            AddCriteria(x => x.MaxScore > 0 && x.ScorePercentage >= query.MinScorePercentage.Value);
        }

        if (query.MaxScorePercentage.HasValue)
        {
            AddCriteria(x => x.MaxScore > 0 && x.ScorePercentage <= query.MaxScorePercentage.Value);
        }

        if (query.IsScored.HasValue)
        {
            AddCriteria(query.IsScored.Value ? x => x.MaxScore > 0 : x => x.MaxScore <= 0);
        }

        if (query.SatisfactionCategory.HasValue)
        {
            switch (query.SatisfactionCategory.Value)
            {
                case SatisfactionCategory.Satisfied:
                    AddCriteria(x => x.MaxScore > 0 && x.ScorePercentage >= SatisfactionCategoryRule.SatisfiedMinimum);
                    break;
                case SatisfactionCategory.Neutral:
                    AddCriteria(x =>
                        x.MaxScore > 0 &&
                        x.ScorePercentage >= SatisfactionCategoryRule.NeutralMinimum &&
                        x.ScorePercentage < SatisfactionCategoryRule.SatisfiedMinimum);
                    break;
                case SatisfactionCategory.Unhappy:
                    AddCriteria(x => x.MaxScore > 0 && x.ScorePercentage < SatisfactionCategoryRule.NeutralMinimum);
                    break;
            }
        }

        if (query.HasComplaint.HasValue)
        {
            AddCriteria(query.HasComplaint.Value
                ? x => x.Answers.Any(answer => answer.QuestionType == QuestionType.Complain)
                : x => !x.Answers.Any(answer => answer.QuestionType == QuestionType.Complain));
        }

        if (query.HasVoice.HasValue)
        {
            AddCriteria(query.HasVoice.Value
                ? x => x.Answers.Any(answer => answer.QuestionType == QuestionType.Voice)
                : x => !x.Answers.Any(answer => answer.QuestionType == QuestionType.Voice));
        }

        if (query.QuestionId.HasValue)
        {
            AddCriteria(x => x.Answers.Any(answer => answer.QuestionId == query.QuestionId.Value));
        }

        ApplyCustomInputFilter(query);

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var searchText = query.SearchText.Trim();
            AddCriteria(x =>
                x.Template.Branch.NameEn.Contains(searchText) ||
                (x.Template.Branch.NameAr != null && x.Template.Branch.NameAr.Contains(searchText)) ||
                x.Template.Branch.Code.Contains(searchText) ||
                x.Template.NameEn.Contains(searchText) ||
                (x.Template.NameAr != null && x.Template.NameAr.Contains(searchText)) ||
                x.Operator.ApplicationUser.NameEn.Contains(searchText) ||
                (x.Operator.ApplicationUser.NameAr != null && x.Operator.ApplicationUser.NameAr.Contains(searchText)) ||
                x.CustomInputValues.Any(value =>
                    value.NameSnapshot.Contains(searchText) ||
                    (value.StringValue != null && value.StringValue.Contains(searchText))));
        }

        if (query.OrderSort == OrderSort.Oldest)
        {
            AddOrderBy(x => x.SubmittedOnUtc);
            AddOrderBy(x => x.Id);
        }
        else
        {
            AddOrderByDescending(x => x.SubmittedOnUtc);
            AddOrderByDescending(x => x.Id);
        }

        ApplyPaging(query.PageNumber, query.PageSize);

        Select(x => new DepartmentSurveyResponsePaginationItemResponse
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
            ScorePercentage = x.ScorePercentage,
            IsScored = x.MaxScore > 0,
            HasComplaint = x.Answers.Any(answer => answer.QuestionType == QuestionType.Complain),
            HasVoice = x.Answers.Any(answer => answer.QuestionType == QuestionType.Voice)
        });
    }

    private void ApplyCustomInputFilter(GetDepartmentSurveyResponsesPaginationQuery query)
    {
        var name = query.CustomInputName?.Trim();
        var valueText = query.CustomInputValue?.Trim();

        if (string.IsNullOrWhiteSpace(name) && !query.CustomInputType.HasValue && string.IsNullOrWhiteSpace(valueText))
        {
            return;
        }

        var hasName = !string.IsNullOrWhiteSpace(name);
        var hasValue = !string.IsNullOrWhiteSpace(valueText);

        if (query.CustomInputType == TemplateCustomInputType.Integer && hasValue)
        {
            var integerValue = int.Parse(valueText!);
            AddCriteria(x => x.CustomInputValues.Any(value =>
                (!hasName || value.NameSnapshot == name) &&
                value.TypeSnapshot == TemplateCustomInputType.Integer &&
                value.IntegerValue == integerValue));
            return;
        }

        if (query.CustomInputType == TemplateCustomInputType.String && hasValue)
        {
            AddCriteria(x => x.CustomInputValues.Any(value =>
                (!hasName || value.NameSnapshot == name) &&
                value.TypeSnapshot == TemplateCustomInputType.String &&
                value.StringValue == valueText));
            return;
        }

        AddCriteria(x => x.CustomInputValues.Any(value =>
            (!hasName || value.NameSnapshot == name) &&
            (!query.CustomInputType.HasValue || value.TypeSnapshot == query.CustomInputType.Value)));
    }
}

internal sealed record DepartmentSurveyResponseCustomInputPreviewDto
{
    public Guid SurveyResponseId { get; init; }
    public string NameSnapshot { get; init; } = string.Empty;
    public TemplateCustomInputType TypeSnapshot { get; init; }
    public string? StringValue { get; init; }
    public int? IntegerValue { get; init; }
}

internal sealed class GetDepartmentSurveyResponseCustomInputPreviewsSpec
    : Specification<SurveyResponseCustomInputValue, DepartmentSurveyResponseCustomInputPreviewDto>
{
    public GetDepartmentSurveyResponseCustomInputPreviewsSpec(IReadOnlyCollection<Guid> responseIds)
    {
        UseNoTracking();
        AddCriteria(x => responseIds.Contains(x.SurveyResponseId));
        AddOrderBy(x => x.TemplateCustomInput.Order);
        AddOrderBy(x => x.Id);

        Select(x => new DepartmentSurveyResponseCustomInputPreviewDto
        {
            SurveyResponseId = x.SurveyResponseId,
            NameSnapshot = x.NameSnapshot,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue
        });
    }
}
