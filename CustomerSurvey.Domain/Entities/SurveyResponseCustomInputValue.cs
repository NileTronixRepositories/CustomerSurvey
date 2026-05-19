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
        public Guid SurveyResponseId { get; private set; }
        public SurveyResponse SurveyResponse { get; private set; } = null!;

        public Guid TemplateCustomInputId { get; private set; }
        public TemplateCustomInput TemplateCustomInput { get; private set; } = null!;

        public string NameSnapshot { get; private set; } = string.Empty;

        public TemplateCustomInputType TypeSnapshot { get; private set; }

        public string? StringValue { get; private set; }

        public int? IntegerValue { get; private set; }

        private SurveyResponseCustomInputValue()
        {
        }

        public static SurveyResponseCustomInputValue CreateStringValue(
            Guid surveyResponseId,
            Guid templateCustomInputId,
            string nameSnapshot,
            string value)
        {
            return new SurveyResponseCustomInputValue
            {
                Id = Guid.NewGuid(),
                SurveyResponseId = surveyResponseId,
                TemplateCustomInputId = templateCustomInputId,
                NameSnapshot = nameSnapshot.Trim(),
                TypeSnapshot = TemplateCustomInputType.String,
                StringValue = value.Trim(),
                IntegerValue = null
            };
        }

        public static SurveyResponseCustomInputValue CreateIntegerValue(
            Guid surveyResponseId,
            Guid templateCustomInputId,
            string nameSnapshot,
            int value)
        {
            return new SurveyResponseCustomInputValue
            {
                Id = Guid.NewGuid(),
                SurveyResponseId = surveyResponseId,
                TemplateCustomInputId = templateCustomInputId,
                NameSnapshot = nameSnapshot.Trim(),
                TypeSnapshot = TemplateCustomInputType.Integer,
                StringValue = null,
                IntegerValue = value
            };
        }
    }
}