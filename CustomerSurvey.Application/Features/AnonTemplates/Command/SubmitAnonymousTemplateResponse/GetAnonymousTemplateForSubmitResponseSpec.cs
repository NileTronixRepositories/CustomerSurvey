using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed class GetAnonymousTemplateForSubmitResponseSpec
        : Specification<AnonymousTemplate, AnonymousTemplateForSubmitResponseDto>
    {
        public GetAnonymousTemplateForSubmitResponseSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x => x.Id == anonymousTemplateId);

            Select(x => new AnonymousTemplateForSubmitResponseDto
            {
                AnonymousTemplateId = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                ActiveFrom = x.ActiveFrom,
                ExpireTo = x.ExpireTo,
                IsActive = x.IsActive
            });
        }
    }
}