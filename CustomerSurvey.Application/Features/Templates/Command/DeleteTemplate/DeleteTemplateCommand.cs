using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.DeleteTemplate
{
    public sealed record DeleteTemplateCommand : ICommand<DeleteTemplateResponse>
    {
        public Guid TemplateId { get; init; }
    }
}