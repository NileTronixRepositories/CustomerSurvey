using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.RestoreGlobalQuestionGroup
{
    public sealed record RestoreGlobalQuestionGroupCommand
      : ICommand<RestoreGlobalQuestionGroupResponse>
    {
        public Guid GroupId { get; init; }
    }
}