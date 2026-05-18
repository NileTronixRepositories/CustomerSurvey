using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.DeleteGlobalQuestionGroup
{
    public sealed record DeleteGlobalQuestionGroupCommand
        : ICommand<DeleteGlobalQuestionGroupResponse>
    {
        public Guid GroupId { get; init; }
    }
}