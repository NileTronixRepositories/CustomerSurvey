using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionDetails
{
    public sealed record GetGlobalQuestionDetailsQuery
        : IQuery<GetGlobalQuestionDetailsResponse>
    {
        public Guid QuestionId { get; init; }
    }
}