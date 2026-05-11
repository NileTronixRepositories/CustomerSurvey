using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.DeleteQuestion
{
    public sealed record DeleteQuestionCommand
       : ICommand<DeleteQuestionResponse>
    {
        public Guid QuestionId { get; init; }
    }
}