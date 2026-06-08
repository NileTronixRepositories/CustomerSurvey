using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed class GetDepartmentOperatorSurveyResponseAnswersSpec
    : Specification<SurveyAnswer, DepartmentOperatorSurveyAnswerDetailsDto>
{
    public GetDepartmentOperatorSurveyResponseAnswersSpec(Guid surveyResponseId)
    {
        AddCriteria(x => x.SurveyResponseId == surveyResponseId);

        Select(x => new DepartmentOperatorSurveyAnswerDetailsDto
        {
            SurveyResponseId = x.SurveyResponseId,
            QuestionId = x.QuestionId,
            QuestionTextEn = x.Question.TextEn,
            QuestionTextAr = x.Question.TextAr,
            QuestionType = x.QuestionType,

            SelectedQuestionOptionId = x.QuestionType == QuestionType.SingleChoice
                ? x.SelectedQuestionOptionId
                : null,

            StarRatingValue = x.QuestionType == QuestionType.StarRating
                ? x.StarRatingValue
                : null,

            SmileValue = x.QuestionType == QuestionType.Smiles
                ? x.SmileValue
                : null,

            TextAnswer = x.QuestionType == QuestionType.Complain
                ? x.TextAnswer
                : null,

            VoiceFileName = x.QuestionType == QuestionType.Voice
                ? x.VoiceFileName
                : null,

            ImageFileName = x.QuestionType == QuestionType.Image
                ? x.ImageFileName
                : null
        });
    }
}
