using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class TemplateCustomInput : Entity<Guid>
    {
        public Guid TemplateId { get; private set; }
        public Template Template { get; private set; } = null!;

        public string Name { get; private set; } = string.Empty;
        public string? LabelEn { get; private set; }
        public string? LabelAr { get; private set; }

        public TemplateCustomInputType Type { get; private set; }

        public bool IsRequired { get; private set; }

        public int? MinLength { get; private set; }
        public int? MaxLength { get; private set; }

        public int? MinValue { get; private set; }
        public int? MaxValue { get; private set; }

        public string? StartWith { get; private set; }

        public int Order { get; private set; }

        public bool IsActive { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        private TemplateCustomInput()
        {
        }

        public static TemplateCustomInput Create(
            Guid templateId,
            string name,
            string? labelEn,
            string? labelAr,
            TemplateCustomInputType type,
            bool isRequired,
            int? minLength,
            int? maxLength,
            int? minValue,
            int? maxValue,
            string? startWith,
            int order,
            Guid createdByApplicationUserId)
        {
            var normalizedValidation = NormalizeValidationByType(
                type,
                minLength,
                maxLength,
                minValue,
                maxValue,
                startWith);

            return new TemplateCustomInput
            {
                Id = Guid.NewGuid(),
                TemplateId = templateId,
                Name = name.Trim(),
                LabelEn = string.IsNullOrWhiteSpace(labelEn) ? null : labelEn.Trim(),
                LabelAr = string.IsNullOrWhiteSpace(labelAr) ? null : labelAr.Trim(),
                Type = type,
                IsRequired = isRequired,
                MinLength = normalizedValidation.MinLength,
                MaxLength = normalizedValidation.MaxLength,
                MinValue = normalizedValidation.MinValue,
                MaxValue = normalizedValidation.MaxValue,
                StartWith = normalizedValidation.StartWith,
                Order = order,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void Update(
            string name,
            string? labelEn,
            string? labelAr,
            TemplateCustomInputType type,
            bool isRequired,
            int? minLength,
            int? maxLength,
            int? minValue,
            int? maxValue,
            string? startWith,
            int order)
        {
            var normalizedValidation = NormalizeValidationByType(
                type,
                minLength,
                maxLength,
                minValue,
                maxValue,
                startWith);

            Name = name.Trim();
            LabelEn = string.IsNullOrWhiteSpace(labelEn) ? null : labelEn.Trim();
            LabelAr = string.IsNullOrWhiteSpace(labelAr) ? null : labelAr.Trim();
            Type = type;
            IsRequired = isRequired;
            MinLength = normalizedValidation.MinLength;
            MaxLength = normalizedValidation.MaxLength;
            MinValue = normalizedValidation.MinValue;
            MaxValue = normalizedValidation.MaxValue;
            StartWith = normalizedValidation.StartWith;
            Order = order;
        }

        public void ChangeOrder(int order)
        {
            Order = order;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Restore()
        {
            IsActive = true;
        }

        private static CustomInputValidationValues NormalizeValidationByType(
            TemplateCustomInputType type,
            int? minLength,
            int? maxLength,
            int? minValue,
            int? maxValue,
            string? startWith)
        {
            return type switch
            {
                TemplateCustomInputType.String => new CustomInputValidationValues
                {
                    MinLength = minLength,
                    MaxLength = maxLength,
                    MinValue = null,
                    MaxValue = null,
                    StartWith = string.IsNullOrWhiteSpace(startWith)
                        ? null
                        : startWith.Trim()
                },

                TemplateCustomInputType.Integer => new CustomInputValidationValues
                {
                    MinLength = null,
                    MaxLength = null,
                    MinValue = minValue,
                    MaxValue = maxValue,
                    StartWith = null
                },

                _ => new CustomInputValidationValues()
            };
        }

        private sealed class CustomInputValidationValues
        {
            public int? MinLength { get; init; }
            public int? MaxLength { get; init; }
            public int? MinValue { get; init; }
            public int? MaxValue { get; init; }
            public string? StartWith { get; init; }
        }
    }
}
