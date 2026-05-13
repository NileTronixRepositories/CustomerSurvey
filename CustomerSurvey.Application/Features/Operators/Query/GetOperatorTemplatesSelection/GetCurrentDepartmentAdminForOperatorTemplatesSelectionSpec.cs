using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection
{
    internal sealed record DepartmentAdminForOperatorTemplatesSelectionDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetCurrentDepartmentAdminForOperatorTemplatesSelectionSpec
        : Specification<DepartmentAdmin, DepartmentAdminForOperatorTemplatesSelectionDto>
    {
        public GetCurrentDepartmentAdminForOperatorTemplatesSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new DepartmentAdminForOperatorTemplatesSelectionDto
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}