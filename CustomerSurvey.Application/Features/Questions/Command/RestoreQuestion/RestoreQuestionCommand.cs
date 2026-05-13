using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.RestoreQuestion
{
    public sealed record RestoreQuestionCommand
      : ICommand<RestoreQuestionResponse>
    {
        public Guid QuestionId { get; init; }
    }
}