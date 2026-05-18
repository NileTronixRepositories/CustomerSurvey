using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination
{
    internal sealed class GetQuestionCreatorsForQuestionsPaginationSpec
       : Specification<ApplicationUser, QuestionPaginationCreatorDto>
    {
        public GetQuestionCreatorsForQuestionsPaginationSpec(
            IReadOnlyCollection<Guid> applicationUserIds)
        {
            AddCriteria(x => applicationUserIds.Contains(x.Id));

            Select(x => new QuestionPaginationCreatorDto
            {
                ApplicationUserId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}