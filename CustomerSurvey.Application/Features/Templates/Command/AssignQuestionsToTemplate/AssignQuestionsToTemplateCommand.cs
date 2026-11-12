using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    public sealed record AssignQuestionsToTemplateCommand
        : ICommand<AssignQuestionsToTemplateResponse>
    {
        public Guid TemplateId { get; init; }

        public IReadOnlyCollection<Guid> QuestionIds { get; init; }
            = Array.Empty<Guid>();

        public IReadOnlyCollection<TemplateQuestionRequirement> QuestionRequirements { get; init; }
            = Array.Empty<TemplateQuestionRequirement>();
    }

    public sealed record TemplateQuestionRequirement(Guid QuestionId, bool? IsRequired);
}