using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Shared.Specs
{
    internal sealed class GetQuestionOptionsByQuestionIdsSpec
        : Specification<QuestionOption, QuestionOptionResponse>
    {
        public GetQuestionOptionsByQuestionIdsSpec(
            IReadOnlyCollection<Guid> questionIds,
            bool activeOnly = true)
        {
            AddCriteria(x => questionIds.Contains(x.QuestionId));

            if (activeOnly)
            {
                AddCriteria(x => x.IsActive);
            }

            AddOrderBy(x => x.Order);

            Select(x => new QuestionOptionResponse
            {
                OptionId = x.Id,
                QuestionId = x.QuestionId,
                TextEn = x.TextEn,
                TextAr = x.TextAr,
                Order = x.Order,
                Value = x.Value,
                IsActive = x.IsActive
            });
        }
    }
}