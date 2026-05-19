using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed class GetTemplateCustomInputsForMyOperatorTemplatesSpec
        : Specification<TemplateCustomInput, TemplateCustomInputForMyOperatorTemplateDto>
    {
        public GetTemplateCustomInputsForMyOperatorTemplatesSpec(
            IReadOnlyCollection<Guid> templateIds)
        {
            AddCriteria(x =>
                templateIds.Contains(x.TemplateId) &&
                x.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateCustomInputForMyOperatorTemplateDto
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
                Order = x.Order
            });
        }
    }
}