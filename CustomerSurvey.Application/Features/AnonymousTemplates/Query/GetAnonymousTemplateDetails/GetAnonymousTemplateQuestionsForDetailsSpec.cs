using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed class GetAnonymousTemplateQuestionsForDetailsSpec
        : Specification<AnonymousTemplateQuestion, AnonymousTemplateDetailsQuestionResponse>
    {
        public GetAnonymousTemplateQuestionsForDetailsSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId);

            AddOrderBy(x => x.Order);

            Select(x => new AnonymousTemplateDetailsQuestionResponse
            {
                AnonymousTemplateQuestionId = x.Id,
                QuestionId = x.QuestionId,

                BranchId = x.Question.BranchId,
                GroupId = x.Question.GroupId,

                GroupNameEn = x.Question.Group.NameEn,
                GroupNameAr = x.Question.Group.NameAr,

                Scope = x.Question.Scope,
                ScopeName = x.Question.Scope.ToString(),
                IsGlobal = x.Question.Scope == QuestionScope.Global,
                IsEditable = x.Question.Scope == QuestionScope.Branch,

                TextEn = x.Question.TextEn,
                TextAr = x.Question.TextAr,

                Type = x.Question.Type,
                TypeName = x.Question.Type.ToString(),

                Order = x.Order,
                IsActive = x.Question.IsActive,

                Options = Array.Empty<AnonymousTemplateDetailsQuestionOptionResponse>()
            });
        }
    }
}