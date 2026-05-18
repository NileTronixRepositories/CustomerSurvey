using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed record TemplateQuestionForMyOperatorDto
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid TemplateId { get; init; }

        public Guid QuestionId { get; init; }

        public Guid? QuestionBranchId { get; init; }

        public Guid GroupId { get; init; }

        public Guid? GroupBranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public int Order { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public string Type { get; init; } = string.Empty;

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }
    }

    internal sealed class GetTemplateQuestionsForMyOperatorTemplatesSpec
        : Specification<TemplateQuestion, TemplateQuestionForMyOperatorDto>
    {
        public GetTemplateQuestionsForMyOperatorTemplatesSpec(
            IReadOnlyCollection<Guid> templateIds)
        {
            AddCriteria(x =>
                templateIds.Contains(x.TemplateId) &&
                x.Question.IsActive &&
                x.Question.Group.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateQuestionForMyOperatorDto
            {
                TemplateQuestionId = x.Id,
                TemplateId = x.TemplateId,
                QuestionId = x.QuestionId,
                QuestionBranchId = x.Question.BranchId,
                GroupId = x.Question.GroupId,
                GroupBranchId = x.Question.Group.BranchId,
                Scope = x.Question.Scope,
                Order = x.Order,
                TextEn = x.Question.TextEn,
                TextAr = x.Question.TextAr,
                Type = x.Question.Type.ToString(),
                GroupNameEn = x.Question.Group.NameEn,
                GroupNameAr = x.Question.Group.NameAr
            });
        }
    }
}