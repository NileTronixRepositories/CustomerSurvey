using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsSelection
{
    internal sealed class GetGlobalQuestionGroupsSelectionSpec
         : Specification<QuestionGroup, GlobalQuestionGroupSelectionResponse>
    {
        public GetGlobalQuestionGroupsSelectionSpec()
        {
            AddCriteria(x =>
                x.Scope == QuestionScope.Global &&
                x.BranchId == null &&
                x.IsActive);

            AddOrderBy(x => x.NameEn);

            Select(x => new GlobalQuestionGroupSelectionResponse
            {
                Id = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                ScopeName = x.Scope.ToString(),
                IsGlobal = x.Scope == QuestionScope.Global,
                IsSelectable = x.Scope == QuestionScope.Global && x.IsActive,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}