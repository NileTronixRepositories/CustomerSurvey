using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsSelection
{
    public sealed record GlobalQuestionGroupSelectionResponse
    {
        public Guid Id { get; init; }

        public Guid? BranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsSelectable { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}