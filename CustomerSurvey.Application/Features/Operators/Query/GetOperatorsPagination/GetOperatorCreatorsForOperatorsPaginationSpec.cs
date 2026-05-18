using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorsPagination
{
    internal sealed class GetOperatorCreatorsForOperatorsPaginationSpec
        : Specification<ApplicationUser, OperatorPaginationCreatorDto>
    {
        public GetOperatorCreatorsForOperatorsPaginationSpec(
            IReadOnlyCollection<Guid> applicationUserIds)
        {
            AddCriteria(x => applicationUserIds.Contains(x.Id));

            Select(x => new OperatorPaginationCreatorDto
            {
                ApplicationUserId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}