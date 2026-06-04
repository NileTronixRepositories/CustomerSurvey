using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed class GetAnonymousTemplateCustomInputsForSubmitSpec
        : Specification<AnonymousTemplateCustomInput, AnonymousTemplateCustomInputForSubmitDto>
    {
        public GetAnonymousTemplateCustomInputsForSubmitSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x =>
                x.AnonymousTemplateId == anonymousTemplateId &&
                x.IsActive);

            Select(x => new AnonymousTemplateCustomInputForSubmitDto
            {
                CustomInputId = x.Id,
                Name = x.Name,
                Type = x.Type,
                IsRequired = x.IsRequired,
                MinLength = x.MinLength,
                MaxLength = x.MaxLength,
                MinValue = x.MinValue,
                MaxValue = x.MaxValue,
                StartWith = x.StartWith,
                IsActive = x.IsActive
            });
        }
    }
}
