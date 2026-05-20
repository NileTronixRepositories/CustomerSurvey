using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate
{
    internal sealed class GetPublicAnonymousTemplateQuestionsSpec
        : Specification<AnonymousTemplateQuestion, PublicAnonymousTemplateQuestionResponse>
    {
        public GetPublicAnonymousTemplateQuestionsSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x =>
                x.AnonymousTemplateId == anonymousTemplateId &&
                x.Question.IsActive &&
                x.Question.Group.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new PublicAnonymousTemplateQuestionResponse
            {
                AnonymousTemplateQuestionId = x.Id,
                QuestionId = x.QuestionId,

                GroupId = x.Question.GroupId,
                GroupNameEn = x.Question.Group.NameEn,
                GroupNameAr = x.Question.Group.NameAr,

                Scope = x.Question.Scope,
                ScopeName = x.Question.Scope.ToString(),
                IsGlobal = x.Question.Scope == QuestionScope.Global,

                TextEn = x.Question.TextEn,
                TextAr = x.Question.TextAr,

                Type = x.Question.Type,
                TypeName = x.Question.Type.ToString(),

                Order = x.Order,
                IsRoot = false,

                Options = Array.Empty<PublicAnonymousTemplateQuestionOptionResponse>()
            });
        }
    }
}