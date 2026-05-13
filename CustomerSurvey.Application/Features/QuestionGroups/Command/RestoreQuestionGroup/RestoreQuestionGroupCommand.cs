using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.RestoreQuestionGroup
{
    public sealed record RestoreQuestionGroupCommand
      : ICommand<RestoreQuestionGroupResponse>
    {
        public Guid GroupId { get; init; }
    }
}