using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsSelection
{
    public sealed record GetGlobalQuestionGroupsSelectionQuery
         : IQuery<IReadOnlyCollection<GlobalQuestionGroupSelectionResponse>>;
}