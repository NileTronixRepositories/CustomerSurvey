using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    public sealed record GetAnonymousTemplateQuestionsSelectionQuery
          : IQuery<GetAnonymousTemplateQuestionsSelectionResponse>
    {
        public Guid AnonymousTemplateId { get; init; }

        public string? SearchText { get; init; }
    }
}