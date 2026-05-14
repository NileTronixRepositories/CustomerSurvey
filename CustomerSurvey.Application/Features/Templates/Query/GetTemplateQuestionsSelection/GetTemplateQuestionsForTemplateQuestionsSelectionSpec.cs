using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Templates.Shared;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed record TemplateQuestionForTemplateQuestionsSelectionDto
    {
        public Guid QuestionId { get; init; }

        public int Order { get; init; }
        public Guid TemplateQuestionId { get; init; }
    }

    internal sealed class GetTemplateQuestionsForTemplateQuestionsSelectionSpec
        : Specification<TemplateQuestion, TemplateQuestionForTemplateQuestionsSelectionDto>
    {
        public GetTemplateQuestionsForTemplateQuestionsSelectionSpec(Guid templateId)
        {
            AddCriteria(x => x.TemplateId == templateId);

            Select(x => new TemplateQuestionForTemplateQuestionsSelectionDto
            {
                QuestionId = x.QuestionId,
                Order = x.Order,
                TemplateQuestionId = x.Id,
            });
        }
    }
}