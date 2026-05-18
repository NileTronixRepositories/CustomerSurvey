using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion
{
    public sealed record UpdateGlobalQuestionResponse
    {
        public Guid QuestionId { get; init; }

        public Guid? BranchId { get; init; }

        public Guid GroupId { get; init; }

        public Guid? GroupBranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsEditable { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public IReadOnlyCollection<QuestionOptionResponse> Options { get; init; }
            = Array.Empty<QuestionOptionResponse>();
    }
}