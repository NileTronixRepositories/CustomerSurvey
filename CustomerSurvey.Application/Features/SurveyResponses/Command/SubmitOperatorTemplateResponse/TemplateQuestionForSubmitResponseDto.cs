using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed record TemplateQuestionForSubmitResponseDto
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid TemplateId { get; init; }

        public Guid QuestionId { get; init; }

        public Guid? QuestionBranchId { get; init; }

        public Guid GroupId { get; init; }

        public Guid? GroupBranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public int Order { get; init; }

        public QuestionType Type { get; init; }
    }
}