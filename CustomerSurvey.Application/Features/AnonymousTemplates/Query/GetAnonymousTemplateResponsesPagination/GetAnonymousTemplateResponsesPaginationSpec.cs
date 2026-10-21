using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    internal sealed class GetAnonymousTemplateResponsesPaginationSpec
        : CustomerSurvey.Application.Features.Reports.Shared.ScoredResponseSpecification<AnonymousSurveyResponse, AnonymousTemplateResponsePaginationItemResponse>
    {
        public GetAnonymousTemplateResponsesPaginationSpec(
            GetAnonymousTemplateResponsesPaginationQuery request)
        : base(request.ScoreCalculationMode)
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

            ApplyIncludedQuestionFilter(request.QuestionId, request.ScoreIncludedOnly);

        Select(x => new AnonymousTemplateResponsePaginationItemResponse
            {
                RequestedModeAvailable = request.ScoreCalculationMode == CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport.ScoreCalculationMode.RootQuestions || (x.Snapshot != null && x.Snapshot.Provenance == SnapshotProvenance.CapturedAtSubmission),
                ScoreDerivation = x.Snapshot != null && x.Snapshot.Provenance == SnapshotProvenance.CapturedAtSubmission ? "SubmissionSnapshot" : "StoredRootBaseline",
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
                        AddCriteria(x => x.MaxScore > 0 && x.ScorePercentage >= (x.AnonymousTemplate == null || x.AnonymousTemplate.Branch == null || x.AnonymousTemplate.Branch.EvaluationSettings == null ? 80m : x.AnonymousTemplate.Branch.EvaluationSettings.SatisfiedMinimum));
                        break;
                    case SatisfactionCategory.Neutral:
                        AddCriteria(x =>
                            x.MaxScore > 0 &&
                            x.ScorePercentage >= (x.AnonymousTemplate == null || x.AnonymousTemplate.Branch == null || x.AnonymousTemplate.Branch.EvaluationSettings == null ? 60m : x.AnonymousTemplate.Branch.EvaluationSettings.NeutralMinimum) &&
                            x.ScorePercentage < (x.AnonymousTemplate == null || x.AnonymousTemplate.Branch == null || x.AnonymousTemplate.Branch.EvaluationSettings == null ? 80m : x.AnonymousTemplate.Branch.EvaluationSettings.SatisfiedMinimum));
                        break;
                    case SatisfactionCategory.Unhappy:
                        AddCriteria(x => x.MaxScore > 0 && x.ScorePercentage < (x.AnonymousTemplate == null || x.AnonymousTemplate.Branch == null || x.AnonymousTemplate.Branch.EvaluationSettings == null ? 60m : x.AnonymousTemplate.Branch.EvaluationSettings.NeutralMinimum));
                        break;
                }
            }

            if (query.QuestionId.HasValue)
            {
                AddCriteria(x => x.Answers.Any(answer => answer.QuestionId == query.QuestionId.Value));
            }

            var customInputId = query.CustomInputId;
            var customInputValue = query.CustomInputValue?.Trim();

            if (customInputId.HasValue || query.CustomInputType.HasValue || !string.IsNullOrWhiteSpace(customInputValue))
            {
                var hasCustomInputId = customInputId.HasValue;
                var hasValue = !string.IsNullOrWhiteSpace(customInputValue);

                if (query.CustomInputType == TemplateCustomInputType.Integer && hasValue)
                {
                    var integerValue = int.Parse(customInputValue!);
                    AddCriteria(x => x.CustomInputValues.Any(value =>
                        (!hasCustomInputId || value.AnonymousTemplateCustomInputId == customInputId.Value) &&
                        value.TypeSnapshot == TemplateCustomInputType.Integer &&
                        value.IntegerValue == integerValue));
                }
                else if (query.CustomInputType == TemplateCustomInputType.String && hasValue)
                {
                    AddCriteria(x => x.CustomInputValues.Any(value =>
                        (!hasCustomInputId || value.AnonymousTemplateCustomInputId == customInputId.Value) &&
                        value.TypeSnapshot == TemplateCustomInputType.String &&
                        value.StringValue == customInputValue));
                }
                else
                {
                    AddCriteria(x => x.CustomInputValues.Any(value =>
                        (!hasCustomInputId || value.AnonymousTemplateCustomInputId == customInputId.Value) &&
                        (!query.CustomInputType.HasValue || value.TypeSnapshot == query.CustomInputType.Value)));
                }
            }
        }
    }
}
