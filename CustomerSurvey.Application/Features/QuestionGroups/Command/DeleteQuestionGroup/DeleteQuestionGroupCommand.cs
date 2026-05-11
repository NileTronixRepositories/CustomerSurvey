using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.DeleteQuestionGroup
{
    public sealed record DeleteQuestionGroupCommand
       : ICommand<DeleteQuestionGroupResponse>
    {
        public Guid GroupId { get; init; }
    }
}