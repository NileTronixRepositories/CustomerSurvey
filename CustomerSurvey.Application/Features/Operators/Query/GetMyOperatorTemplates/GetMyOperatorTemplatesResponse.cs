using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    public sealed record GetMyOperatorTemplatesResponse
    {
        public Guid OperatorId { get; init; }

        public Guid DepartmentId { get; init; }

        public int TemplatesCount { get; init; }

        public IReadOnlyCollection<MyOperatorTemplateItemResponse> Templates { get; init; }
            = Array.Empty<MyOperatorTemplateItemResponse>();
    }

    public sealed record MyOperatorTemplateItemResponse
    {
        public Guid TemplateId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public Guid BranchId { get; init; }

        public string BranchNameEn { get; init; } = string.Empty;

        public string? BranchNameAr { get; init; }

        public string BranchCode { get; init; } = string.Empty;

        public int QuestionsCount { get; init; }

        public IReadOnlyCollection<MyOperatorTemplateQuestionResponse> Questions { get; init; }
            = Array.Empty<MyOperatorTemplateQuestionResponse>();
    }

    public sealed record MyOperatorTemplateQuestionResponse
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public int Order { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public string Type { get; init; } = string.Empty;

        public Guid GroupId { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }

        public IReadOnlyCollection<MyOperatorQuestionOptionResponse> Options { get; init; }
            = Array.Empty<MyOperatorQuestionOptionResponse>();
    }
}