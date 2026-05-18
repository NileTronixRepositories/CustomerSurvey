using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    internal sealed record TemplateQuestionForTemplateDetailsDto
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid TemplateId { get; init; }

        public Guid QuestionId { get; init; }

        public Guid? QuestionBranchId { get; init; }

        public Guid GroupId { get; init; }

        public Guid? GroupBranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public int Order { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public bool IsActive { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }
    }
}