using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.CreateQuestionGroup
{
    public sealed record CreateQuestionGroupCommand
        : ICommand<CreateQuestionGroupResponse>
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}