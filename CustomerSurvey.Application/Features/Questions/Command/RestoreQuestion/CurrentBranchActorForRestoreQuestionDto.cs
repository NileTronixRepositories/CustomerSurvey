using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.RestoreQuestion
{
    internal sealed record CurrentBranchActorForRestoreQuestionDto
    {
        public Guid BranchId { get; init; }
    }
}