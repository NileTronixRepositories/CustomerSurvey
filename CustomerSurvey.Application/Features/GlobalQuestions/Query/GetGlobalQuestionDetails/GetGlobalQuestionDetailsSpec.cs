using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionDetails
{
    internal sealed class GetGlobalQuestionDetailsSpec
       : Specification<Question, GetGlobalQuestionDetailsResponse>
    {
        public GetGlobalQuestionDetailsSpec(Guid questionId)
        {
            AddCriteria(x =>
                x.Id == questionId &&
                x.Scope == QuestionScope.Global &&
                x.BranchId == null &&
                x.Group.Scope == QuestionScope.Global &&
                x.Group.BranchId == null);

            Select(x => new GetGlobalQuestionDetailsResponse
            {
                QuestionId = x.Id,
                BranchId = x.BranchId,

                GroupId = x.GroupId,
                GroupBranchId = x.Group.BranchId,
                GroupNameEn = x.Group.NameEn,
                GroupNameAr = x.Group.NameAr,

                Scope = x.Scope,
                ScopeName = x.Scope.ToString(),
                IsGlobal = x.Scope == QuestionScope.Global,
                IsEditable = x.Scope == QuestionScope.Global,

                TextEn = x.TextEn,
                TextAr = x.TextAr,

                Type = x.Type,
                TypeName = x.Type.ToString(),

                IsActive = x.IsActive,
                CreatedOnUtc = x.CreatedOnUtc,
                ModifiedOnUtc = x.ModifiedOnUtc
            });
        }
    }
}