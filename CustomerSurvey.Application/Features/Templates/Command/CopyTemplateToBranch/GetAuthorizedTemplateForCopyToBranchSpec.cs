using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;

internal sealed class GetAuthorizedTemplateForCopyToBranchSpec
    : Specification<Template>
{
    public GetAuthorizedTemplateForCopyToBranchSpec(Guid templateId)
    {
        AddCriteria(x => x.Id == templateId);

        AddInclude(x => x.Include(template => template.CustomInputs));
        AddInclude(x => x
            .Include(template => template.TemplateQuestions)
            .ThenInclude(templateQuestion => templateQuestion.Question)
            .ThenInclude(question => question.Group));
        AddInclude(x => x
            .Include(template => template.TemplateQuestions)
            .ThenInclude(templateQuestion => templateQuestion.Question)
            .ThenInclude(question => question.Options));
    }
}
