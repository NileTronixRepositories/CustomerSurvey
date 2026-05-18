using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.DeleteGlobalQuestion
{
    public sealed record DeleteGlobalQuestionCommand
         : ICommand<DeleteGlobalQuestionResponse>
    {
        public Guid QuestionId { get; init; }
    }
}