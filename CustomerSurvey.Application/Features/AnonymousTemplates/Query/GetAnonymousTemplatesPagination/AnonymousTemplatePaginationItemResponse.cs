using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplatesPagination
{
    public sealed record AnonymousTemplatePaginationItemResponse
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid? BranchId { get; init; }

        public string? BranchNameEn { get; init; }

        public string? BranchNameAr { get; init; }

        public AnonymousTemplateScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public bool IsActive { get; init; }

        public bool IsArchived { get; init; }

        public string? LogoPath { get; init; }

        public string? PublicUrl { get; init; }

        public string? QrCode { get; init; }

        public int QuestionsCount { get; init; }

        public int CustomInputsCount { get; init; }

        public int ResponsesCount { get; init; }

        public Guid CreatedByApplicationUserId { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }
}
