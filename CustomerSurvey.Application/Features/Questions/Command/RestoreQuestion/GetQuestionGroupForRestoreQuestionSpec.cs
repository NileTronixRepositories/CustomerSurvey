using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.RestoreQuestion
{
    internal sealed record QuestionGroupForRestoreQuestionDto
    {
        public Guid GroupId { get; init; }

        public Guid BranchId { get; init; }

        public bool IsActive { get; init; }
    }

    internal sealed class GetQuestionGroupForRestoreQuestionSpec
        : Specification<QuestionGroup, QuestionGroupForRestoreQuestionDto>
    {
        public GetQuestionGroupForRestoreQuestionSpec(
            Guid groupId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == groupId &&
                x.BranchId == branchId);

            Select(x => new QuestionGroupForRestoreQuestionDto
            {
                GroupId = x.Id,
                BranchId = x.BranchId,
                IsActive = x.IsActive
            });
        }
    }
}