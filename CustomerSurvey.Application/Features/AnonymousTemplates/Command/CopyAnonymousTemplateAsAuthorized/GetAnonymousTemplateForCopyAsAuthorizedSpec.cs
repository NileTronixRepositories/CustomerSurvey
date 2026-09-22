using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CopyAnonymousTemplateAsAuthorized;

internal sealed class GetAnonymousTemplateForCopyAsAuthorizedSpec : Specification<AnonymousTemplate>
{
    public GetAnonymousTemplateForCopyAsAuthorizedSpec(Guid templateId, Guid branchId)
    {
        AddCriteria(x => x.Id == templateId && x.BranchId == branchId);
        AddInclude(x => x.Include(template => template.Questions));
    }
}
