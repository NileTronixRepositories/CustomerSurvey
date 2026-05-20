using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetAnonymousSurveyResponseAnswersDetailsSpec
        : Specification<AnonymousSurveyAnswer, AnonymousTemplateResponseAnswerDetailsResponse>
    {
        public GetAnonymousSurveyResponseAnswersDetailsSpec(Guid responseId)
        {
            AddCriteria(x => x.AnonymousSurveyResponseId == responseId);

            Select(x => new AnonymousTemplateResponseAnswerDetailsResponse
            {
                AnswerId = x.Id,
                AnonymousTemplateQuestionId = x.AnonymousTemplateQuestionId,
                QuestionId = x.QuestionId,

                QuestionTextEn = string.Empty,
                QuestionTextAr = null,
                QuestionType = default,
                QuestionTypeName = string.Empty,
                QuestionOrder = 0,

                SelectedQuestionOptionId = x.SelectedQuestionOptionId,

                SelectedOptionTextEn = null,
                SelectedOptionTextAr = null,
                SelectedOptionValue = null,

                StarRatingValue = x.StarRatingValue,
                SmileValue = x.SmileValue,
                TextAnswer = x.TextAnswer,
                VoiceFileName = x.VoiceFileName
            });
        }
    }
}