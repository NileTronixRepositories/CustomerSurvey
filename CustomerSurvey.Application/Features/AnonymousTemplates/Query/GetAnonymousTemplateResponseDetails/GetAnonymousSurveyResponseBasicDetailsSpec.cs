using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetAnonymousSurveyResponseBasicDetailsSpec
        : Specification<AnonymousSurveyResponse, AnonymousSurveyResponseBasicDetailsDto>
    {
        public GetAnonymousSurveyResponseBasicDetailsSpec(
            Guid anonymousTemplateId,
            Guid responseId)
        {
            AddCriteria(x =>
                x.Id == responseId &&
                x.AnonymousTemplateId == anonymousTemplateId);

            Select(x => new AnonymousSurveyResponseBasicDetailsDto
            {
                AnonymousSurveyResponseId = x.Id,
                AnonymousTemplateId = x.AnonymousTemplateId,
                SubmittedOnUtc = x.SubmittedOnUtc,
                ActualScore = x.ActualScore,
                MaxScore = x.MaxScore,
                ScorePercentage = x.ScorePercentage,
                AnswersCount = x.Answers.Count,
                CustomInputValuesCount = x.CustomInputValues.Count
            });
        }
    }
}