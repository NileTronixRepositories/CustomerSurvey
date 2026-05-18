using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsPagination
{
    internal sealed record QuestionGroupPaginationItemDto
    {
        public Guid GroupId { get; init; }

        public Guid? BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public bool IsActive { get; init; }

        public int QuestionsCount { get; init; }

        public Guid CreatedByApplicationUserId { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }
}