using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate
{
    internal sealed class GetCurrentSuperAdminForCreateAnonymousTemplateSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForCreateAnonymousTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}