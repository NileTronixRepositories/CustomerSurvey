using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesForSelection
{
    internal sealed record CurrentDepartmentAdminForTemplatesSelectionDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetCurrentDepartmentAdminForTemplatesSelectionSpec
        : Specification<DepartmentAdmin, CurrentDepartmentAdminForTemplatesSelectionDto>
    {
        public GetCurrentDepartmentAdminForTemplatesSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentDepartmentAdminForTemplatesSelectionDto
            {
                DepartmentAdminId = x.Id,
                DepartmentId = x.DepartmentId
            });
        }
    }
}