using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class AnonymousSurveyResponseCustomInputValue : Entity<Guid>
    {
        private const int LegacyNameSnapshotMaxLength = 100;

        public Guid AnonymousSurveyResponseId { get; private set; }
        public AnonymousSurveyResponse AnonymousSurveyResponse { get; private set; } = null!;

        public Guid AnonymousTemplateCustomInputId { get; private set; }
        public AnonymousTemplateCustomInput AnonymousTemplateCustomInput { get; private set; } = null!;

        public string NameSnapshot { get; private set; } = string.Empty;

        public string? LabelEnSnapshot { get; private set; }

        public string? LabelArSnapshot { get; private set; }

        public TemplateCustomInputType TypeSnapshot { get; private set; }

        public string? StringValue { get; private set; }

        public int? IntegerValue { get; private set; }

        private AnonymousSurveyResponseCustomInputValue()
        {
        }

        public static AnonymousSurveyResponseCustomInputValue CreateStringValue(
            Guid anonymousSurveyResponseId,
            Guid anonymousTemplateCustomInputId,
            string? labelEnSnapshot,
            string? labelArSnapshot,
            string value)
        {
            return new AnonymousSurveyResponseCustomInputValue
            {
                Id = Guid.NewGuid(),
                AnonymousSurveyResponseId = anonymousSurveyResponseId,
                AnonymousTemplateCustomInputId = anonymousTemplateCustomInputId,
                NameSnapshot = ResolveLegacyNameSnapshot(labelEnSnapshot, labelArSnapshot),
                LabelEnSnapshot = NormalizeLabel(labelEnSnapshot),
                LabelArSnapshot = NormalizeLabel(labelArSnapshot),
                TypeSnapshot = TemplateCustomInputType.String,
                StringValue = value.Trim(),
                IntegerValue = null
            };
        }

        public static AnonymousSurveyResponseCustomInputValue CreateIntegerValue(
            Guid anonymousSurveyResponseId,
            Guid anonymousTemplateCustomInputId,
            string? labelEnSnapshot,
            string? labelArSnapshot,
            int value)
        {
            return new AnonymousSurveyResponseCustomInputValue
            {
                Id = Guid.NewGuid(),
                AnonymousSurveyResponseId = anonymousSurveyResponseId,
                AnonymousTemplateCustomInputId = anonymousTemplateCustomInputId,
                NameSnapshot = ResolveLegacyNameSnapshot(labelEnSnapshot, labelArSnapshot),
                LabelEnSnapshot = NormalizeLabel(labelEnSnapshot),
                LabelArSnapshot = NormalizeLabel(labelArSnapshot),
                TypeSnapshot = TemplateCustomInputType.Integer,
                StringValue = null,
                IntegerValue = value
            };
        }

        private static string? NormalizeLabel(string? label)
            => string.IsNullOrWhiteSpace(label) ? null : label.Trim();

        private static string ResolveLegacyNameSnapshot(string? labelEn, string? labelAr)
        {
            var value = NormalizeLabel(labelEn) ?? NormalizeLabel(labelAr) ?? string.Empty;
            return value.Length <= LegacyNameSnapshotMaxLength
                ? value
                : value[..LegacyNameSnapshotMaxLength];
        }
    }
}
