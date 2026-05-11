using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    internal sealed record TemplatePaginationItemDto
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public TemplateStatus Status { get; init; }

        public bool IsActive { get; init; }

        public int QuestionsCount { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }
}