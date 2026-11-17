using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Enums
{
    public enum QuestionType
    {
        SingleChoice = 1,
        Voice = 2,
        StarRating = 3,
        Complain = 4,
        Smiles = 5,
        Image = 6,
        FreeText = 7,
        Rating1To10 = 8,
        Dropdown = 9
    }

    public static class QuestionTypeExtensions
    {
        public static bool HasOptions(this QuestionType type) =>
            type is QuestionType.SingleChoice or QuestionType.Dropdown;
    }
}
