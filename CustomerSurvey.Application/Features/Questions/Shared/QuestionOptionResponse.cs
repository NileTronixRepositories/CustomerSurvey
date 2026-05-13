using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Shared
{
    public sealed record QuestionOptionResponse
    {
        public Guid OptionId { get; init; }

        public Guid QuestionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public bool IsActive { get; init; }
    }
}