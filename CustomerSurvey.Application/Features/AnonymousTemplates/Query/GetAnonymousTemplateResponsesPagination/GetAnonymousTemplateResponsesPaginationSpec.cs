using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    internal sealed class GetAnonymousTemplateResponsesPaginationSpec
        : Specification<AnonymousSurveyResponse, AnonymousTemplateResponsePaginationItemResponse>
    {
        public GetAnonymousTemplateResponsesPaginationSpec(
            GetAnonymousTemplateResponsesPaginationQuery request)
        {
            AddCriteria(x => x.AnonymousTemplateId == request.AnonymousTemplateId);

            if (request.FromDate.HasValue)
            {
                AddCriteria(x => x.SubmittedOnUtc >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                AddCriteria(x => x.SubmittedOnUtc < request.ToDate.Value);
            }

            if (request.MinScorePercentage.HasValue)
            {
                AddCriteria(x => x.ScorePercentage >= request.MinScorePercentage.Value);
            }

            if (request.MaxScorePercentage.HasValue)
            {
                AddCriteria(x => x.ScorePercentage <= request.MaxScorePercentage.Value);
            }

            ApplyDrillDownFilters(request);

            if (request.OrderSort == OrderSort.Oldest)
            {
                AddOrderBy(x => x.SubmittedOnUtc);
            }
            else
            {
                AddOrderByDescending(x => x.SubmittedOnUtc);
            }

            EnableTotalCount();

            ApplyPaging(
                request.PageNumber,
                request.PageSize);

            Select(x => new AnonymousTemplateResponsePaginationItemResponse
            {
                AnonymousSurveyResponseId = x.Id,
                AnonymousTemplateId = x.AnonymousTemplateId,
                SubmittedOnUtc = x.SubmittedOnUtc,

                ActualScore = x.ActualScore,
                MaxScore = x.MaxScore,
                ScorePercentage = x.ScorePercentage,
                IsScored = x.MaxScore > 0,

                AnswersCount = x.Answers.Count,
                CustomInputValuesCount = x.CustomInputValues.Count
            });
        }

        private void ApplyDrillDownFilters(GetAnonymousTemplateResponsesPaginationQuery query)
        {
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

            if (query.QuestionId.HasValue)
            {
                AddCriteria(x => x.Answers.Any(answer => answer.QuestionId == query.QuestionId.Value));
            }

            var customInputName = query.CustomInputName?.Trim();
            var customInputValue = query.CustomInputValue?.Trim();

            if (!string.IsNullOrWhiteSpace(customInputName) || query.CustomInputType.HasValue || !string.IsNullOrWhiteSpace(customInputValue))
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
}
