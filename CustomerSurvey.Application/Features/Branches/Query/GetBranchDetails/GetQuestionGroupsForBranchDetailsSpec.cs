using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    internal sealed record QuestionGroupForBranchDetailsDto
    {
        public Guid GroupId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }

    internal sealed class GetQuestionGroupsForBranchDetailsSpec
        : Specification<QuestionGroup, QuestionGroupForBranchDetailsDto>
    {
        public GetQuestionGroupsForBranchDetailsSpec(Guid branchId)
        {
            AddCriteria(x => x.BranchId == branchId);

            AddOrderBy(x => x.NameEn);

            Select(x => new QuestionGroupForBranchDetailsDto
            {
                GroupId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}