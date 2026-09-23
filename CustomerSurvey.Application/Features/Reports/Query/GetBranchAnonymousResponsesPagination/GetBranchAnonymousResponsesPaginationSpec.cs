using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchAnonymousResponsesPagination;

internal sealed class GetBranchAnonymousResponsesPaginationSpec
    : Specification<AnonymousSurveyResponse, BranchAnonymousResponsePaginationItemResponse>
{
    private const int CustomInputsPreviewCount = 5;

    public GetBranchAnonymousResponsesPaginationSpec(
        Guid branchId,
        DateTime? fromUtc,
        DateTime? toExclusiveUtc,
        GetBranchAnonymousResponsesPaginationQuery query)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.AnonymousTemplate.Scope == AnonymousTemplateScope.Branch &&
            x.AnonymousTemplate.BranchId == branchId);

        if (query.AnonymousTemplateId.HasValue)
        {
            AddCriteria(x => x.AnonymousTemplateId == query.AnonymousTemplateId.Value);
        }

        if (fromUtc.HasValue)
        {
            AddCriteria(x => x.SubmittedOnUtc >= fromUtc.Value);
        }

        if (toExclusiveUtc.HasValue)
        {
            AddCriteria(x => x.SubmittedOnUtc < toExclusiveUtc.Value);
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

        ApplyDrillDownFilters(query);

        if (query.HasComplaint.HasValue)
        {
            if (query.HasComplaint.Value)
            {
                AddCriteria(x =>
                    x.Answers.Any(answer =>
                        answer.QuestionType == QuestionType.Complain &&
                        answer.TextAnswer != null &&
                        answer.TextAnswer != string.Empty));
            }
            else
            {
                AddCriteria(x =>
                    !x.Answers.Any(answer =>
                        answer.QuestionType == QuestionType.Complain &&
                        answer.TextAnswer != null &&
                        answer.TextAnswer != string.Empty));
            }
        }

        if (query.HasVoice.HasValue)
        {
            if (query.HasVoice.Value)
            {
                AddCriteria(x =>
                    x.Answers.Any(answer =>
                        answer.QuestionType == QuestionType.Voice &&
                        answer.VoiceFileName != null &&
                        answer.VoiceFileName != string.Empty));
            }
            else
            {
                AddCriteria(x =>
                    !x.Answers.Any(answer =>
                        answer.QuestionType == QuestionType.Voice &&
                        answer.VoiceFileName != null &&
                        answer.VoiceFileName != string.Empty));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var searchText = query.SearchText.Trim();

            AddCriteria(x =>
                x.AnonymousTemplate.NameEn.Contains(searchText) ||
                (x.AnonymousTemplate.NameAr != null && x.AnonymousTemplate.NameAr.Contains(searchText)) ||
                x.CustomInputValues.Any(value =>
                    value.NameSnapshot.Contains(searchText) ||
                    (value.StringValue != null && value.StringValue.Contains(searchText)) ||
                    (value.AnonymousTemplateCustomInput.LabelEn != null &&
                        value.AnonymousTemplateCustomInput.LabelEn.Contains(searchText)) ||
                    (value.AnonymousTemplateCustomInput.LabelAr != null &&
                        value.AnonymousTemplateCustomInput.LabelAr.Contains(searchText))));
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

        Select(x => new BranchAnonymousResponsePaginationItemResponse
        {
            AnonymousSurveyResponseId = x.Id,
            AnonymousTemplateId = x.AnonymousTemplateId,
            AnonymousTemplateNameEn = x.AnonymousTemplate.NameEn,
            AnonymousTemplateNameAr = x.AnonymousTemplate.NameAr,
            SubmittedOnUtc = x.SubmittedOnUtc,
            ScorePercentage = x.ScorePercentage,
            IsScored = x.MaxScore > 0,

            HasComplaint = x.Answers.Any(answer =>
                answer.QuestionType == QuestionType.Complain &&
                answer.TextAnswer != null &&
                answer.TextAnswer != string.Empty),

            HasVoice = x.Answers.Any(answer =>
                answer.QuestionType == QuestionType.Voice &&
                answer.VoiceFileName != null &&
                answer.VoiceFileName != string.Empty),

            CustomInputsPreview = x.CustomInputValues
                .OrderBy(value => value.AnonymousTemplateCustomInput.Order)
                .ThenBy(value => value.Id)
                .Take(CustomInputsPreviewCount)
                .Select(value => new BranchAnonymousResponseCustomInputPreviewResponse
                {
                    Name = value.NameSnapshot,
                    LabelEn = value.AnonymousTemplateCustomInput.LabelEn,
                    LabelAr = value.AnonymousTemplateCustomInput.LabelAr,
                    Value = value.TypeSnapshot == TemplateCustomInputType.String
                        ? value.StringValue ?? string.Empty
                        : value.TypeSnapshot == TemplateCustomInputType.Integer && value.IntegerValue.HasValue
                            ? value.IntegerValue.Value.ToString()
                            : string.Empty
                })
                .ToArray()
        });
    }

    private void ApplyDrillDownFilters(GetBranchAnonymousResponsesPaginationQuery query)
    {
        if (query.IsScored.HasValue)
        {
            AddCriteria(query.IsScored.Value
                ? x => x.MaxScore > 0
                : x => x.MaxScore <= 0);
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

        if (query.QuestionId.HasValue)
        {
            AddCriteria(x => x.Answers.Any(answer => answer.QuestionId == query.QuestionId.Value));
        }

        var customInputName = query.CustomInputName?.Trim();
        var customInputValue = query.CustomInputValue?.Trim();

        if (!string.IsNullOrWhiteSpace(customInputName) ||
            query.CustomInputType.HasValue ||
            !string.IsNullOrWhiteSpace(customInputValue))
        {
            var hasName = !string.IsNullOrWhiteSpace(customInputName);
            var hasValue = !string.IsNullOrWhiteSpace(customInputValue);

            if (query.CustomInputType == TemplateCustomInputType.Integer && hasValue)
            {
                var integerValue = int.Parse(customInputValue!);
                AddCriteria(x => x.CustomInputValues.Any(value =>
                    (!hasName || value.NameSnapshot == customInputName) &&
                    value.TypeSnapshot == TemplateCustomInputType.Integer &&
                    value.IntegerValue == integerValue));
            }
            else if (query.CustomInputType == TemplateCustomInputType.String && hasValue)
            {
                AddCriteria(x => x.CustomInputValues.Any(value =>
                    (!hasName || value.NameSnapshot == customInputName) &&
                    value.TypeSnapshot == TemplateCustomInputType.String &&
                    value.StringValue == customInputValue));
            }
            else
            {
                AddCriteria(x => x.CustomInputValues.Any(value =>
                    (!hasName || value.NameSnapshot == customInputName) &&
                    (!query.CustomInputType.HasValue || value.TypeSnapshot == query.CustomInputType.Value)));
            }
        }
    }
}
