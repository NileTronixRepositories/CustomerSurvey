using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed class GetTemplateCustomInputsForSubmitResponseSpec
        : Specification<TemplateCustomInput, TemplateCustomInputForSubmitResponseDto>
    {
        public GetTemplateCustomInputsForSubmitResponseSpec(Guid templateId)
        {
            AddCriteria(x =>
                x.TemplateId == templateId &&
                x.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateCustomInputForSubmitResponseDto
            {
                CustomInputId = x.Id,
                TemplateId = x.TemplateId,
                Name = x.Name,
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