using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsPagination
{
    internal sealed class GetQuestionGroupCreatorsForQuestionGroupsPaginationSpec
      : Specification<ApplicationUser, QuestionGroupPaginationCreatorDto>
    {
        public GetQuestionGroupCreatorsForQuestionGroupsPaginationSpec(
            IReadOnlyCollection<Guid> applicationUserIds)
        {
            AddCriteria(x => applicationUserIds.Contains(x.Id));

            Select(x => new QuestionGroupPaginationCreatorDto
            {
                ApplicationUserId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}