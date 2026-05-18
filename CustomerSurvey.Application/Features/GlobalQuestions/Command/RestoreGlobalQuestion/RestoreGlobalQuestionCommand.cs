using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.RestoreGlobalQuestion
{
    public sealed record RestoreGlobalQuestionCommand
         : ICommand<RestoreGlobalQuestionResponse>
    {
        public Guid QuestionId { get; init; }
    }
}