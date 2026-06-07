using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.CreateBranchArea
{
    internal sealed class GetRoleByNameForCreateBranchAreaSpec
        : Specification<Role>
    {
        public GetRoleByNameForCreateBranchAreaSpec(string roleName)
        {
            AddCriteria(x => x.Name == roleName);
        }
    }
}
