using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.AssignTemplatesToOperator
{
    internal sealed class GetCurrentOperatorTemplatesForAssignTemplatesToOperatorSpec
        : Specification<OperatorTemplate>
    {
        public GetCurrentOperatorTemplatesForAssignTemplatesToOperatorSpec(Guid operatorId)
        {
            AddCriteria(x => x.OperatorId == operatorId);
        }
    }
}