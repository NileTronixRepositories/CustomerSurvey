using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    internal sealed class GetAvailableQuestionsForAnonymousTemplateSelectionSpec
        : Specification<Question, AnonymousTemplateQuestionSelectionItemResponse>
    {
        public GetAvailableQuestionsForAnonymousTemplateSelectionSpec(
            AnonymousTemplateForQuestionsSelectionDto anonymousTemplate,
            string? searchText)
        {
            AddCriteria(x => x.IsActive);

            AddCriteria(x => x.Group.IsActive);

            if (anonymousTemplate.Scope == AnonymousTemplateScope.Global)
            {
                AddCriteria(x => x.Scope == QuestionScope.Global);
            }
            else
            {
                AddCriteria(x =>
                    x.Scope == QuestionScope.Global ||
                    (
                        x.Scope == QuestionScope.Branch &&
                        x.BranchId == anonymousTemplate.BranchId
                    ));
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizedSearchText = searchText.Trim();

                AddCriteria(x =>
                    x.TextEn.Contains(normalizedSearchText) ||
                    (x.TextAr != null && x.TextAr.Contains(normalizedSearchText)) ||
                    x.Group.NameEn.Contains(normalizedSearchText) ||
                    (x.Group.NameAr != null && x.Group.NameAr.Contains(normalizedSearchText)));
            }

            AddOrderBy(x => x.Group.NameEn);
            AddOrderBy(x => x.TextEn);

            Select(x => new AnonymousTemplateQuestionSelectionItemResponse
            {
                QuestionId = x.Id,
                AnonymousTemplateQuestionId = null,

                BranchId = x.BranchId,

                GroupId = x.GroupId,
                GroupNameEn = x.Group.NameEn,
                GroupNameAr = x.Group.NameAr,

                Scope = x.Scope,
                ScopeName = x.Scope.ToString(),
                IsGlobal = x.Scope == QuestionScope.Global,
                IsEditable = x.Scope == QuestionScope.Branch,

                IsSelected = false,
                SelectedOrder = null,

                TextEn = x.TextEn,
                TextAr = x.TextAr,

                Type = x.Type,
                TypeName = x.Type.ToString(),

                IsActive = x.IsActive,

                Options = Array.Empty<AnonymousTemplateQuestionSelectionOptionResponse>()
            });
        }
    }
}