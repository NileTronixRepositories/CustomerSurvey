using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Enums
{
    public enum QuestionType
    {
        Text = 1,
        Rating = 2,
        YesNo = 3,
        SingleChoice = 4,
        MultipleChoice = 5
    }
}