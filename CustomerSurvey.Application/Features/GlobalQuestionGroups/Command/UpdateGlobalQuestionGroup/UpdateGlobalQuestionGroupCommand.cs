using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.UpdateGlobalQuestionGroup
{
    public sealed record UpdateGlobalQuestionGroupCommand
        : ICommand<UpdateGlobalQuestionGroupResponse>
    {
        public Guid GroupId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}