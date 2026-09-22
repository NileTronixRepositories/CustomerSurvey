using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;

internal sealed class GetAnonymousTemplateForCopyToBranchSpec
    : Specification<AnonymousTemplate>
{
    public GetAnonymousTemplateForCopyToBranchSpec(Guid anonymousTemplateId)
    {
        AddCriteria(x => x.Id == anonymousTemplateId);

        AddInclude(x => x.Include(template => template.CustomInputs));
        AddInclude(x => x
            .Include(template => template.Questions)
            .ThenInclude(templateQuestion => templateQuestion.Question)
            .ThenInclude(question => question.Group));
        AddInclude(x => x
            .Include(template => template.Questions)
            .ThenInclude(templateQuestion => templateQuestion.Question)
            .ThenInclude(question => question.Options));
    }
}
