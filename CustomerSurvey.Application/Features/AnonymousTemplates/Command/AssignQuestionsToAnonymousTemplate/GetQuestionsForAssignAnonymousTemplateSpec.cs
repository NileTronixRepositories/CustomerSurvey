using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    internal sealed class GetQuestionsForAssignAnonymousTemplateSpec
        : Specification<Question, QuestionForAssignToAnonymousTemplateDto>
    {
        public GetQuestionsForAssignAnonymousTemplateSpec(
            IReadOnlyCollection<Guid> questionIds)
        {
            AddCriteria(x => questionIds.Contains(x.Id));

            Select(x => new QuestionForAssignToAnonymousTemplateDto
            {
                QuestionId = x.Id,
                BranchId = x.BranchId,

                GroupId = x.GroupId,
                GroupNameEn = x.Group.NameEn,
                GroupNameAr = x.Group.NameAr,

                Scope = x.Scope,

                TextEn = x.TextEn,
                TextAr = x.TextAr,

                Type = x.Type,

                IsActive = x.IsActive,
                GroupIsActive = x.Group.IsActive
            });
        }
    }
}