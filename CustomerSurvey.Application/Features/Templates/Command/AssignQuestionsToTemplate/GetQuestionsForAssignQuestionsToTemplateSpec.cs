using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed record QuestionForAssignQuestionsToTemplateDto
    {
        public Guid QuestionId { get; init; }

        public Guid? BranchId { get; init; }

        public Guid GroupId { get; init; }

        public QuestionScope Scope { get; init; }
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
                x.IsActive &&
                x.Group.IsActive &&
                (
                    (x.Scope == QuestionScope.Branch && x.BranchId == branchId) ||
                    (x.Scope == QuestionScope.Global && x.BranchId == null)
                ));

            Select(x => new QuestionForAssignQuestionsToTemplateDto
            {
                QuestionId = x.Id,
                BranchId = x.BranchId,
                GroupId = x.GroupId,
                Scope = x.Scope
            });
        }
    }
}