using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.CreateOperator
{
    internal sealed class GetOperatorRoleForCreateOperatorSpec
        : Specification<Role>
    {
        public GetOperatorRoleForCreateOperatorSpec()
        {
            AddCriteria(x => x.Name == "Operator");
        }
    }
}