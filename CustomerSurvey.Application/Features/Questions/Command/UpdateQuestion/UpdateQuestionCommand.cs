using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion
{
    public sealed record UpdateQuestionCommand
        : ICommand<UpdateQuestionResponse>
    {
        public Guid QuestionId { get; init; }

        public Guid GroupId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public IReadOnlyCollection<QuestionOptionCommandItem> Options { get; init; }
    = Array.Empty<QuestionOptionCommandItem>();
    }
}