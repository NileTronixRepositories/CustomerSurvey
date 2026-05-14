using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    internal sealed record TemplateQuestionForManageTemplateQuestionConditionsDto
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public Guid GroupId { get; init; }

        public QuestionType QuestionType { get; init; }

        public bool QuestionIsActive { get; init; }

        public bool QuestionGroupIsActive { get; init; }
    }

    internal sealed class GetTemplateQuestionsForManageTemplateQuestionConditionsSpec
        : Specification<TemplateQuestion, TemplateQuestionForManageTemplateQuestionConditionsDto>
    {
        public GetTemplateQuestionsForManageTemplateQuestionConditionsSpec(Guid templateId)
        {
            AddCriteria(x => x.TemplateId == templateId);

            Select(x => new TemplateQuestionForManageTemplateQuestionConditionsDto
            {
                TemplateQuestionId = x.Id,
                QuestionId = x.QuestionId,
                GroupId = x.Question.GroupId,
                QuestionType = x.Question.Type,
                QuestionIsActive = x.Question.IsActive,
                QuestionGroupIsActive = x.Question.Group.IsActive
            });
        }
    }
}