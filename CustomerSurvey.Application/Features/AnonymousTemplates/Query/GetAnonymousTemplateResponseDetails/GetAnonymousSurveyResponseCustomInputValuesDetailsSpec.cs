using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetAnonymousSurveyResponseCustomInputValuesDetailsSpec
        : Specification<AnonymousSurveyResponseCustomInputValue, AnonymousTemplateResponseCustomInputValueDetailsResponse>
    {
        public GetAnonymousSurveyResponseCustomInputValuesDetailsSpec(Guid responseId)
        {
            AddCriteria(x => x.AnonymousSurveyResponseId == responseId);

            Select(x => new AnonymousTemplateResponseCustomInputValueDetailsResponse
            {
                CustomInputValueId = x.Id,
                AnonymousTemplateCustomInputId = x.AnonymousTemplateCustomInputId,
                LabelEnSnapshot = x.LabelEnSnapshot,
                LabelArSnapshot = x.LabelArSnapshot,

                Type = x.IntegerValue.HasValue
                    ? TemplateCustomInputType.Integer
                    : TemplateCustomInputType.String,

                TypeName = x.IntegerValue.HasValue
                    ? TemplateCustomInputType.Integer.ToString()
                    : TemplateCustomInputType.String.ToString(),

                StringValue = x.StringValue,
                IntegerValue = x.IntegerValue
            });
        }
    }
}
