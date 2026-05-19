using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate
{
    internal sealed class UpdateTemplateCommandValidator
        : AbstractValidator<UpdateTemplateCommand>
    {
        private const int CustomInputNameMaxLength = 100;
        private const int CustomInputLabelMaxLength = 200;
        private const int CustomInputStringMaxLength = 3000;

        public UpdateTemplateCommandValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateTemplate_TemplateId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateTemplate_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateTemplate_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateTemplate_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage(ErrorMessage.UpdateTemplate_Description_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.ActiveFrom)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateTemplate_ActiveFrom_Required);

            RuleFor(x => x)
                .Must(x => !x.ExpireTo.HasValue || x.ExpireTo.Value > x.ActiveFrom)
                .WithMessage(ErrorMessage.UpdateTemplate_ExpireTo_MustBeAfterActiveFrom);

            RuleFor(x => x.CustomInputs)
                .NotNull()
                .WithMessage(ErrorMessage.UpdateTemplate_CustomInputs_Invalid);

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueCustomInputIds)
                .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_Id_Duplicated)
                .When(x => x.CustomInputs is not null && x.CustomInputs.Count > 0);

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueCustomInputNames)
                .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_Name_Duplicated)
                .When(x => x.CustomInputs is not null && x.CustomInputs.Count > 0);

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueCustomInputOrders)
                .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_Order_Duplicated)
                .When(x => x.CustomInputs is not null && x.CustomInputs.Count > 0);

            RuleForEach(x => x.CustomInputs)
                .ChildRules(customInput =>
                {
                    customInput.RuleFor(x => x.Name)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_Name_Required)
                        .MaximumLength(CustomInputNameMaxLength)
                        .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_Name_MaxLength);

                    customInput.RuleFor(x => x.LabelEn)
                        .MaximumLength(CustomInputLabelMaxLength)
                        .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_LabelEn_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.LabelEn));

                    customInput.RuleFor(x => x.LabelAr)
                        .MaximumLength(CustomInputLabelMaxLength)
                        .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_LabelAr_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.LabelAr));

                    customInput.RuleFor(x => x.Type)
                        .IsInEnum()
                        .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_Type_Invalid);

                    customInput.RuleFor(x => x.Order)
                        .GreaterThan(0)
                        .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_Order_Invalid);

                    customInput.RuleFor(x => x)
                        .Must(BeValidStringValidation)
                        .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_StringValidation_Invalid)
                        .When(x => x.Type == TemplateCustomInputType.String);

                    customInput.RuleFor(x => x)
                        .Must(BeValidIntegerValidation)
                        .WithMessage(ErrorMessage.UpdateTemplate_CustomInput_IntegerValidation_Invalid)
                        .When(x => x.Type == TemplateCustomInputType.Integer);
                });
        }

        private static bool HaveUniqueCustomInputIds(
            IReadOnlyCollection<UpdateTemplateCustomInputCommandItem> customInputs)
        {
            var ids = customInputs
                .Where(x => x.CustomInputId.HasValue)
                .Select(x => x.CustomInputId!.Value)
                .ToArray();

            return ids.Length == ids.Distinct().Count();
        }

        private static bool HaveUniqueCustomInputNames(
            IReadOnlyCollection<UpdateTemplateCustomInputCommandItem> customInputs)
        {
            var names = customInputs
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => x.Name.Trim().ToLowerInvariant())
                .ToArray();

            return names.Length == names.Distinct().Count();
        }

        private static bool HaveUniqueCustomInputOrders(
            IReadOnlyCollection<UpdateTemplateCustomInputCommandItem> customInputs)
        {
            var orders = customInputs
                .Select(x => x.Order)
                .Where(x => x > 0)
                .ToArray();

            return orders.Length == orders.Distinct().Count();
        }

        private static bool BeValidStringValidation(UpdateTemplateCustomInputCommandItem customInput)
        {
            if (customInput.MinValue.HasValue || customInput.MaxValue.HasValue)
            {
                return false;
            }

            if (customInput.MinLength.HasValue && customInput.MinLength.Value < 0)
            {
                return false;
            }

            if (customInput.MaxLength.HasValue &&
                (customInput.MaxLength.Value <= 0 || customInput.MaxLength.Value > CustomInputStringMaxLength))
            {
                return false;
            }

            if (customInput.MinLength.HasValue &&
                customInput.MaxLength.HasValue &&
                customInput.MaxLength.Value < customInput.MinLength.Value)
            {
                return false;
            }

            return true;
        }

        private static bool BeValidIntegerValidation(UpdateTemplateCustomInputCommandItem customInput)
        {
            if (customInput.MinLength.HasValue || customInput.MaxLength.HasValue)
            {
                return false;
            }

            if (customInput.MinValue.HasValue &&
                customInput.MaxValue.HasValue &&
                customInput.MaxValue.Value < customInput.MinValue.Value)
            {
                return false;
            }

            return true;
        }
    }
}