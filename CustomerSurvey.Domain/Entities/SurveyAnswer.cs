using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class SurveyAnswer : Entity<Guid>
    {
        public Guid SurveyResponseId { get; private set; }
        public SurveyResponse SurveyResponse { get; private set; } = null!;

        public Guid QuestionId { get; private set; }
        public Question Question { get; private set; } = null!;

        public QuestionType QuestionType { get; private set; }

        public Guid? SelectedQuestionOptionId { get; private set; }
        public QuestionOption? SelectedQuestionOption { get; private set; }

        public int? StarRatingValue { get; private set; }

        public int? SmileValue { get; private set; }

        public string? TextAnswer { get; private set; }

        public string? VoiceFileName { get; private set; }

        private SurveyAnswer()
        {
        }

        public static SurveyAnswer CreateSingleChoice(
            Guid surveyResponseId,
            Guid questionId,
            Guid selectedQuestionOptionId)
        {
            return new SurveyAnswer
            {
                Id = Guid.NewGuid(),
                SurveyResponseId = surveyResponseId,
                QuestionId = questionId,
                QuestionType = QuestionType.SingleChoice,
                SelectedQuestionOptionId = selectedQuestionOptionId
            };
        }

        public static SurveyAnswer CreateVoice(
            Guid surveyResponseId,
            Guid questionId,
            string voiceFileName)
        {
            return new SurveyAnswer
            {
                Id = Guid.NewGuid(),
                SurveyResponseId = surveyResponseId,
                QuestionId = questionId,
                QuestionType = QuestionType.Voice,
                VoiceFileName = voiceFileName.Trim()
            };
        }

        public static SurveyAnswer CreateStarRating(
            Guid surveyResponseId,
            Guid questionId,
            int value)
        {
            return new SurveyAnswer
            {
                Id = Guid.NewGuid(),
                SurveyResponseId = surveyResponseId,
                QuestionId = questionId,
                QuestionType = QuestionType.StarRating,
                StarRatingValue = value
            };
        }

        public static SurveyAnswer CreateComplain(
            Guid surveyResponseId,
            Guid questionId,
            string textAnswer)
        {
            return new SurveyAnswer
            {
                Id = Guid.NewGuid(),
                SurveyResponseId = surveyResponseId,
                QuestionId = questionId,
                QuestionType = QuestionType.Complain,
                TextAnswer = textAnswer.Trim()
            };
        }

        public static SurveyAnswer CreateSmiles(
            Guid surveyResponseId,
            Guid questionId,
            int value)
        {
            return new SurveyAnswer
            {
                Id = Guid.NewGuid(),
                SurveyResponseId = surveyResponseId,
                QuestionId = questionId,
                QuestionType = QuestionType.Smiles,
                SmileValue = value
            };
        }
    }
}