using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.DeleteQuestion
{
    public sealed record DeleteQuestionResponse
    {
        public Guid QuestionId { get; init; }

        public Guid BranchId { get; init; }

        public Guid GroupId { get; init; }

        public bool IsActive { get; init; }
    }
}