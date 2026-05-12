using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    public sealed record GetTemplateQuestionsSelectionResponse
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public string TemplateNameEn { get; init; } = string.Empty;

        public string? TemplateNameAr { get; init; }

        public string Status { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public IReadOnlyCollection<TemplateQuestionsSelectionGroupResponse> Groups { get; init; }
            = Array.Empty<TemplateQuestionsSelectionGroupResponse>();
    }

    public sealed record TemplateQuestionsSelectionGroupResponse
    {
        public Guid GroupId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public IReadOnlyCollection<TemplateQuestionsSelectionQuestionResponse> Questions { get; init; }
            = Array.Empty<TemplateQuestionsSelectionQuestionResponse>();
    }

    public sealed record TemplateQuestionsSelectionQuestionResponse
    {
        public Guid QuestionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public string Type { get; init; } = string.Empty;

        public bool IsSelected { get; init; }

        public int? Order { get; init; }
    }
}