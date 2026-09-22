using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateAsAnonymous;

internal sealed class GetTemplateForCopyAsAnonymousSpec : Specification<Template>
{
    public GetTemplateForCopyAsAnonymousSpec(Guid templateId, Guid branchId)
    {
        AddCriteria(x => x.Id == templateId && x.BranchId == branchId);
        AddInclude(x => x.Include(template => template.TemplateQuestions));
    }
}
