using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.CreateGlobalQuestionGroup
{
    public sealed record CreateGlobalQuestionGroupCommand
      : ICommand<CreateGlobalQuestionGroupResponse>
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}