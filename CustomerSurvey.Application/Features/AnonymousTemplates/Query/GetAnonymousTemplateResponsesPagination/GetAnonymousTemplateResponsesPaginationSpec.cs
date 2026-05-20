using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

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
    }
}