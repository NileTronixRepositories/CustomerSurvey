using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsPagination
{
    public sealed record GlobalQuestionGroupPaginationItemResponse
    {
        public Guid GroupId { get; init; }

        public Guid? BranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsEditable { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public bool IsActive { get; init; }

        public int QuestionsCount { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }
}