using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignGlobalAnonymousTemplateToBranch;

internal sealed class GetGlobalAnonymousTemplateForAssignmentSpec : Specification<AnonymousTemplate>
{
    public GetGlobalAnonymousTemplateForAssignmentSpec(Guid templateId)
    {
        AddCriteria(x => x.Id == templateId);
        AddInclude(x => x
            .Include(template => template.CustomInputs)
            .Include(template => template.Questions));
    }
}
