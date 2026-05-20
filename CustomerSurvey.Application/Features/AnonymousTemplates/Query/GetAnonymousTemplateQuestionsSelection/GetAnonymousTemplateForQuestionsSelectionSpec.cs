using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    internal sealed class GetAnonymousTemplateForQuestionsSelectionSpec
        : Specification<AnonymousTemplate, AnonymousTemplateForQuestionsSelectionDto>
    {
        public GetAnonymousTemplateForQuestionsSelectionSpec(
            Guid anonymousTemplateId,
            bool isSuperAdmin,
            Guid? currentBranchId)
        {
            AddCriteria(x => x.Id == anonymousTemplateId);

            if (isSuperAdmin)
            {
                AddCriteria(x => x.Scope == AnonymousTemplateScope.Global);
            }
            else
            {
                AddCriteria(x =>
                    x.Scope == AnonymousTemplateScope.Branch &&
                    x.BranchId == currentBranchId);
            }

            Select(x => new AnonymousTemplateForQuestionsSelectionDto
            {
                AnonymousTemplateId = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                IsActive = x.IsActive
            });
        }
    }
}