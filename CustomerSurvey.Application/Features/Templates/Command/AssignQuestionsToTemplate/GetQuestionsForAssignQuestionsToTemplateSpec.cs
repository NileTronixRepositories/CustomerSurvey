using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed record QuestionForAssignQuestionsToTemplateDto
    {
        public Guid QuestionId { get; init; }
    }

    internal sealed class GetQuestionsForAssignQuestionsToTemplateSpec
        : Specification<Question, QuestionForAssignQuestionsToTemplateDto>
    {
        public GetQuestionsForAssignQuestionsToTemplateSpec(
            IReadOnlyCollection<Guid> questionIds,
            Guid branchId)
        {
            AddCriteria(x =>
                questionIds.Contains(x.Id) &&
                x.BranchId == branchId &&
                x.IsActive);

            Select(x => new QuestionForAssignQuestionsToTemplateDto
            {
                QuestionId = x.Id
            });
        }
    }
}