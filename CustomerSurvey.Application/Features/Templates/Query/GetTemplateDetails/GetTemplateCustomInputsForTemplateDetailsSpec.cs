using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    internal sealed class GetTemplateCustomInputsForTemplateDetailsSpec
        : Specification<TemplateCustomInput, TemplateCustomInputForTemplateDetailsDto>
    {
        public GetTemplateCustomInputsForTemplateDetailsSpec(Guid templateId)
        {
            AddCriteria(x =>
                x.TemplateId == templateId &&
                x.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateCustomInputForTemplateDetailsDto
            {
                CustomInputId = x.Id,
                TemplateId = x.TemplateId,
                Name = x.Name,
                LabelEn = x.LabelEn,
                LabelAr = x.LabelAr,
                Type = x.Type,
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
