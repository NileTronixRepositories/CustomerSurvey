using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    public sealed record AssignQuestionsToTemplateResponse
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public int QuestionsCount { get; init; }

        public IReadOnlyCollection<AssignedTemplateQuestionResponse> Questions { get; init; }
            = Array.Empty<AssignedTemplateQuestionResponse>();
    }

    public sealed record AssignedTemplateQuestionResponse
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public int Order { get; init; }
    }
}