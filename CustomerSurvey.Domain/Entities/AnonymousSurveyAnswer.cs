using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class AnonymousSurveyAnswer : Entity<Guid>
    {
        public Guid AnonymousSurveyResponseId { get; private set; }
        public AnonymousSurveyResponse AnonymousSurveyResponse { get; private set; } = null!;

        public Guid AnonymousTemplateQuestionId { get; private set; }
        public AnonymousTemplateQuestion AnonymousTemplateQuestion { get; private set; } = null!;

        public Guid QuestionId { get; private set; }
        public Question Question { get; private set; } = null!;

        public QuestionType QuestionType { get; private set; }

        public Guid? SelectedQuestionOptionId { get; private set; }
        public QuestionOption? SelectedQuestionOption { get; private set; }

        public int? StarRatingValue { get; private set; }

        public int? SmileValue { get; private set; }

        public string? TextAnswer { get; private set; }

        public string? VoiceFileName { get; private set; }

        private AnonymousSurveyAnswer()
        {
        }

        public static AnonymousSurveyAnswer CreateSingleChoice(
            Guid anonymousSurveyResponseId,
            Guid anonymousTemplateQuestionId,
            Guid questionId,
            Guid selectedQuestionOptionId)
        {
            return new AnonymousSurveyAnswer
            {
                Id = Guid.NewGuid(),
                AnonymousSurveyResponseId = anonymousSurveyResponseId,
                AnonymousTemplateQuestionId = anonymousTemplateQuestionId,
                QuestionId = questionId,
                QuestionType = QuestionType.SingleChoice,
                SelectedQuestionOptionId = selectedQuestionOptionId
            };
        }

        public static AnonymousSurveyAnswer CreateVoice(
            Guid anonymousSurveyResponseId,
            Guid anonymousTemplateQuestionId,
            Guid questionId,
            string voiceFileName)
        {
            return new AnonymousSurveyAnswer
            {
                Id = Guid.NewGuid(),
                AnonymousSurveyResponseId = anonymousSurveyResponseId,
                AnonymousTemplateQuestionId = anonymousTemplateQuestionId,
                QuestionId = questionId,
                QuestionType = QuestionType.Voice,
                VoiceFileName = voiceFileName.Trim()
            };
        }

        public static AnonymousSurveyAnswer CreateStarRating(
            Guid anonymousSurveyResponseId,
            Guid anonymousTemplateQuestionId,
            Guid questionId,
            int value)
        {
            return new AnonymousSurveyAnswer
            {
                Id = Guid.NewGuid(),
                AnonymousSurveyResponseId = anonymousSurveyResponseId,
                AnonymousTemplateQuestionId = anonymousTemplateQuestionId,
                QuestionId = questionId,
                QuestionType = QuestionType.StarRating,
                StarRatingValue = value
            };
        }

        public static AnonymousSurveyAnswer CreateComplain(
            Guid anonymousSurveyResponseId,
            Guid anonymousTemplateQuestionId,
            Guid questionId,
            string textAnswer)
        {
            return new AnonymousSurveyAnswer
            {
                Id = Guid.NewGuid(),
                AnonymousSurveyResponseId = anonymousSurveyResponseId,
                AnonymousTemplateQuestionId = anonymousTemplateQuestionId,
                QuestionId = questionId,
                QuestionType = QuestionType.Complain,
                TextAnswer = textAnswer.Trim()
            };
        }

        public static AnonymousSurveyAnswer CreateSmiles(
            Guid anonymousSurveyResponseId,
            Guid anonymousTemplateQuestionId,
            Guid questionId,
            int value)
        {
            return new AnonymousSurveyAnswer
            {
                Id = Guid.NewGuid(),
                AnonymousSurveyResponseId = anonymousSurveyResponseId,
                AnonymousTemplateQuestionId = anonymousTemplateQuestionId,
                QuestionId = questionId,
                QuestionType = QuestionType.Smiles,
                SmileValue = value
            };
        }
    }
}