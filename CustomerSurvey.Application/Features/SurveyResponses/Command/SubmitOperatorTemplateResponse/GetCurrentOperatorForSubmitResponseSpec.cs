using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed class GetCurrentOperatorForSubmitResponseSpec
        : Specification<Operator, CurrentOperatorForSubmitResponseDto>
    {
        public GetCurrentOperatorForSubmitResponseSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentOperatorForSubmitResponseDto
            {
                OperatorId = x.Id,
                DepartmentId = x.DepartmentId,
                ApplicationUserId = x.ApplicationUserId
            });
        }
    }
}