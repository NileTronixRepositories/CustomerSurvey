using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplate
{
    public sealed record DeleteAnonymousTemplateCommand
       : ICommand<DeleteAnonymousTemplateResponse>
    {
        public Guid AnonymousTemplateId { get; init; }
    }
}