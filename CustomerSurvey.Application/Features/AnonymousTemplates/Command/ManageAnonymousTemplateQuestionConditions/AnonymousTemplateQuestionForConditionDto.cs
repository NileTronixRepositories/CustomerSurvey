using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    internal sealed record AnonymousTemplateQuestionForConditionDto
    {
        public Guid AnonymousTemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public QuestionType QuestionType { get; init; }

        public bool QuestionIsActive { get; init; }
    }
}