using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class AnonymousSurveyResponseCustomInputValue : Entity<Guid>
    {
        public Guid AnonymousSurveyResponseId { get; private set; }
        public AnonymousSurveyResponse AnonymousSurveyResponse { get; private set; } = null!;

        public Guid AnonymousTemplateCustomInputId { get; private set; }
        public AnonymousTemplateCustomInput AnonymousTemplateCustomInput { get; private set; } = null!;

        public string NameSnapshot { get; private set; } = string.Empty;

        public TemplateCustomInputType TypeSnapshot { get; private set; }

        public string? StringValue { get; private set; }

        public int? IntegerValue { get; private set; }

        private AnonymousSurveyResponseCustomInputValue()
        {
        }

        public static AnonymousSurveyResponseCustomInputValue CreateStringValue(
            Guid anonymousSurveyResponseId,
            Guid anonymousTemplateCustomInputId,
            string nameSnapshot,
            string value)
        {
            return new AnonymousSurveyResponseCustomInputValue
            {
                Id = Guid.NewGuid(),
                AnonymousSurveyResponseId = anonymousSurveyResponseId,
                AnonymousTemplateCustomInputId = anonymousTemplateCustomInputId,
                NameSnapshot = nameSnapshot.Trim(),
                TypeSnapshot = TemplateCustomInputType.String,
                StringValue = value.Trim(),
                IntegerValue = null
            };
        }

        public static AnonymousSurveyResponseCustomInputValue CreateIntegerValue(
            Guid anonymousSurveyResponseId,
            Guid anonymousTemplateCustomInputId,
            string nameSnapshot,
            int value)
        {
            return new AnonymousSurveyResponseCustomInputValue
            {
                Id = Guid.NewGuid(),
                AnonymousSurveyResponseId = anonymousSurveyResponseId,
                AnonymousTemplateCustomInputId = anonymousTemplateCustomInputId,
                NameSnapshot = nameSnapshot.Trim(),
                TypeSnapshot = TemplateCustomInputType.Integer,
                StringValue = null,
                IntegerValue = value
            };
        }
    }
}