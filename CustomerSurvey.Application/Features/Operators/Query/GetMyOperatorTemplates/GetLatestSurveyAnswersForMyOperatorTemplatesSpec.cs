using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed class GetLatestSurveyAnswersForMyOperatorTemplatesSpec
        : Specification<SurveyAnswer, LatestSurveyAnswerForMyOperatorTemplateDto>
    {
        public GetLatestSurveyAnswersForMyOperatorTemplatesSpec(
            IReadOnlyCollection<Guid> surveyResponseIds)
        {
            AddCriteria(x => surveyResponseIds.Contains(x.SurveyResponseId));

            Select(x => new LatestSurveyAnswerForMyOperatorTemplateDto
            {
                SurveyResponseId = x.SurveyResponseId,
                QuestionId = x.QuestionId,
                QuestionType = x.QuestionType,
                SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                SelectedOptionTextEn = x.SelectedQuestionOption == null
                    ? null
                    : x.SelectedQuestionOption.TextEn,
                SelectedOptionTextAr = x.SelectedQuestionOption == null
                    ? null
                    : x.SelectedQuestionOption.TextAr,
                StarRatingValue = x.StarRatingValue,
                SmileValue = x.SmileValue,
                TextAnswer = x.TextAnswer,
                VoiceFileName = x.VoiceFileName,
                ImageFileName = x.ImageFileName
            });
        }
    }
}
