using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed record AnonymousTemplateQuestionForResponseDetailsDto
    {
        public Guid AnonymousTemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public string QuestionTextEn { get; init; } = string.Empty;

        public string? QuestionTextAr { get; init; }

        public QuestionType QuestionType { get; init; }

        public int QuestionOrder { get; init; }
    }
}