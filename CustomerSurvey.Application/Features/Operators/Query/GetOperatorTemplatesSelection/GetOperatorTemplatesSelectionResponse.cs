using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection
{
    public sealed record GetOperatorTemplatesSelectionResponse
    {
        public Guid OperatorId { get; init; }

        public int SelectedTemplatesCount { get; init; }

        public int AvailableTemplatesCount { get; init; }

        public IReadOnlyCollection<OperatorTemplateSelectionItemResponse> SelectedTemplates { get; init; }
            = Array.Empty<OperatorTemplateSelectionItemResponse>();

        public IReadOnlyCollection<OperatorTemplateSelectionItemResponse> AvailableTemplates { get; init; }
            = Array.Empty<OperatorTemplateSelectionItemResponse>();
    }

    public sealed record OperatorTemplateSelectionItemResponse
    {
        public Guid TemplateId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public Guid BranchId { get; init; }

        public string BranchNameEn { get; init; } = string.Empty;

        public string? BranchNameAr { get; init; }

        public string BranchCode { get; init; } = string.Empty;

        public string? LogoPath { get; init; }

        public int QuestionsCount { get; init; }
        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public IReadOnlyCollection<OperatorTemplateSelectionQuestionResponse> Questions { get; init; }
            = Array.Empty<OperatorTemplateSelectionQuestionResponse>();
    }

    public sealed record OperatorTemplateSelectionQuestionResponse
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public int Order { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public string Type { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public Guid GroupId { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }
    }
}
