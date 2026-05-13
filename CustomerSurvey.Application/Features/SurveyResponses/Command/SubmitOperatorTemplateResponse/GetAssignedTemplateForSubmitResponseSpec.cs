using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed class GetAssignedTemplateForSubmitResponseSpec
        : Specification<OperatorTemplate, AssignedTemplateForSubmitResponseDto>
    {
        public GetAssignedTemplateForSubmitResponseSpec(
            Guid operatorId,
            Guid templateId)
        {
            AddCriteria(x =>
                x.OperatorId == operatorId &&
                x.TemplateId == templateId);

            Select(x => new AssignedTemplateForSubmitResponseDto
            {
                TemplateId = x.TemplateId,
                TemplateIsActive = x.Template.IsActive
            });
        }
    }
}