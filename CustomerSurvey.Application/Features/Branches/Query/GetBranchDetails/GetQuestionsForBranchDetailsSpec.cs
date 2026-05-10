using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    internal sealed record QuestionForBranchDetailsDto
    {
        public Guid GroupId { get; init; }

        public Guid QuestionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public bool IsActive { get; init; }
    }

    internal sealed class GetQuestionsForBranchDetailsSpec
        : Specification<Question, QuestionForBranchDetailsDto>
    {
        public GetQuestionsForBranchDetailsSpec(Guid branchId)
        {
            AddCriteria(x => x.BranchId == branchId);

            AddOrderBy(x => x.TextEn);

            Select(x => new QuestionForBranchDetailsDto
            {
                GroupId = x.GroupId,
                QuestionId = x.Id,
                TextEn = x.TextEn,
                TextAr = x.TextAr,
                Type = x.Type,
                IsActive = x.IsActive
            });
        }
    }
}