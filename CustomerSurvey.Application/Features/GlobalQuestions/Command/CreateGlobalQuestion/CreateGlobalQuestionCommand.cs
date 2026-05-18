using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.CreateGlobalQuestion
{
    public sealed record CreateGlobalQuestionCommand
         : ICommand<CreateGlobalQuestionResponse>
    {
        public Guid GroupId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public IReadOnlyCollection<CreateGlobalQuestionOptionCommandItem> Options { get; init; }
            = Array.Empty<CreateGlobalQuestionOptionCommandItem>();
    }

    public sealed record CreateGlobalQuestionOptionCommandItem
    {
        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public int Value { get; init; }
    }
}