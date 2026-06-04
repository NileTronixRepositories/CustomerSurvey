using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed class GetAnonymousTemplateCustomInputsForDetailsSpec
        : Specification<AnonymousTemplateCustomInput, AnonymousTemplateDetailsCustomInputResponse>
    {
        public GetAnonymousTemplateCustomInputsForDetailsSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x =>
                x.AnonymousTemplateId == anonymousTemplateId &&
                x.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new AnonymousTemplateDetailsCustomInputResponse
            {
                CustomInputId = x.Id,
                Name = x.Name,
                LabelEn = x.LabelEn,
                LabelAr = x.LabelAr,
                Type = x.Type,
                TypeName = x.Type.ToString(),
                IsRequired = x.IsRequired,
                MinLength = x.MinLength,
                MaxLength = x.MaxLength,
                MinValue = x.MinValue,
                MaxValue = x.MaxValue,
                StartWith = x.StartWith,
                Order = x.Order,
                IsActive = x.IsActive
            });
        }
    }
}
