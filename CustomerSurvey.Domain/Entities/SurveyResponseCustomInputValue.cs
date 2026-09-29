using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class SurveyResponseCustomInputValue : Entity<Guid>
    {
        private const int LegacyNameSnapshotMaxLength = 100;

        public Guid SurveyResponseId { get; private set; }
        public SurveyResponse SurveyResponse { get; private set; } = null!;

        public Guid TemplateCustomInputId { get; private set; }
        public TemplateCustomInput TemplateCustomInput { get; private set; } = null!;

        public string NameSnapshot { get; private set; } = string.Empty;

        public string? LabelEnSnapshot { get; private set; }

        public string? LabelArSnapshot { get; private set; }

        public TemplateCustomInputType TypeSnapshot { get; private set; }

        public string? StringValue { get; private set; }

        public int? IntegerValue { get; private set; }

        private SurveyResponseCustomInputValue()
        {
        }

        public static SurveyResponseCustomInputValue CreateStringValue(
            Guid surveyResponseId,
            Guid templateCustomInputId,
            string? labelEnSnapshot,
            string? labelArSnapshot,
            string value)
        {
            return new SurveyResponseCustomInputValue
            {
                Id = Guid.NewGuid(),
                SurveyResponseId = surveyResponseId,
                TemplateCustomInputId = templateCustomInputId,
                NameSnapshot = ResolveLegacyNameSnapshot(labelEnSnapshot, labelArSnapshot),
                LabelEnSnapshot = NormalizeLabel(labelEnSnapshot),
                LabelArSnapshot = NormalizeLabel(labelArSnapshot),
                TypeSnapshot = TemplateCustomInputType.String,
                StringValue = value.Trim(),
                IntegerValue = null
            };
        }

        public static SurveyResponseCustomInputValue CreateIntegerValue(
            Guid surveyResponseId,
            Guid templateCustomInputId,
            string? labelEnSnapshot,
            string? labelArSnapshot,
            int value)
        {
            return new SurveyResponseCustomInputValue
            {
                Id = Guid.NewGuid(),
                SurveyResponseId = surveyResponseId,
                TemplateCustomInputId = templateCustomInputId,
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
