using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsForSelection
{
    internal sealed class GetQuestionGroupsForSelectionSpec
        : Specification<QuestionGroup, QuestionGroupSelectionResponse>
    {
        public GetQuestionGroupsForSelectionSpec(Guid branchId)
        {
            AddCriteria(x =>
                x.BranchId == branchId &&
                x.IsActive);

            AddOrderBy(x => x.NameEn);

            Select(x => new QuestionGroupSelectionResponse
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}