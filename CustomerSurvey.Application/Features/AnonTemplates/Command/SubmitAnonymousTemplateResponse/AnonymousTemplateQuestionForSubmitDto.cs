using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed record AnonymousTemplateQuestionForSubmitDto
    {
        public Guid AnonymousTemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public QuestionType QuestionType { get; init; }

        public int Order { get; init; }

        public bool QuestionIsActive { get; init; }

        public bool GroupIsActive { get; init; }
    }
}