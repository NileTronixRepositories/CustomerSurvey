using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Enums
{
    public enum UserType
    {
        SuperAdmin = 1,
        BranchAdmin = 2,
        BranchUser = 3,
        DepartmentAdmin = 4,
        Operator = 5,
        BranchArea = 6
    }
}
