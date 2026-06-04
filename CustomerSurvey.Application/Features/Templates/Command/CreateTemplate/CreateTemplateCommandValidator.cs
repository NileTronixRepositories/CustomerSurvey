using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.CreateTemplate
{
    internal sealed class CreateTemplateCommandValidator
         : AbstractValidator<CreateTemplateCommand>
    {
        private const int CustomInputNameMaxLength = 100;
        private const int CustomInputLabelMaxLength = 200;
        private const int CustomInputStringMaxLength = 3000;
        private const int CustomInputStartWithMaxLength = 100;

        public CreateTemplateCommandValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateTemplate_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateTemplate_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateTemplate_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage(ErrorMessage.CreateTemplate_Description_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.ActiveFrom)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateTemplate_ActiveFrom_Required);

            RuleFor(x => x)
                .Must(x => !x.ExpireTo.HasValue || x.ExpireTo.Value > x.ActiveFrom)
                .WithMessage(ErrorMessage.CreateTemplate_ExpireTo_MustBeAfterActiveFrom);

            RuleFor(x => x.CustomInputs)
                .NotNull()
                .WithMessage(ErrorMessage.CreateTemplate_CustomInputs_Invalid);

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueCustomInputNames)
                .WithMessage(ErrorMessage.CreateTemplate_CustomInput_Name_Duplicated)
                .When(x => x.CustomInputs is not null && x.CustomInputs.Count > 0);

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueCustomInputOrders)
                .WithMessage(ErrorMessage.CreateTemplate_CustomInput_Order_Duplicated)
                .When(x => x.CustomInputs is not null && x.CustomInputs.Count > 0);

            RuleForEach(x => x.CustomInputs)
                .ChildRules(customInput =>
                {
                    customInput.RuleFor(x => x.Name)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_Name_Required)
                        .MaximumLength(CustomInputNameMaxLength)
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_Name_MaxLength);

                    customInput.RuleFor(x => x.LabelEn)
                        .MaximumLength(CustomInputLabelMaxLength)
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_LabelEn_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.LabelEn));

                    customInput.RuleFor(x => x.LabelAr)
                        .MaximumLength(CustomInputLabelMaxLength)
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_LabelAr_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.LabelAr));

                    customInput.RuleFor(x => x.Type)
                        .IsInEnum()
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_Type_Invalid);

                    customInput.RuleFor(x => x.Order)
                        .GreaterThan(0)
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_Order_Invalid);

                    customInput.RuleFor(x => x)
                        .Must(x => x.Type == TemplateCustomInputType.String || x.StartWith is null)
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_StartWith_NotAllowed);

                    customInput.RuleFor(x => x.StartWith)
                        .Must(x => !string.IsNullOrWhiteSpace(x))
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_StartWith_Empty)
                        .When(x => x.StartWith is not null);

                    customInput.RuleFor(x => x.StartWith)
                        .MaximumLength(CustomInputStartWithMaxLength)
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_StartWith_MaxLength)
                        .When(x => x.StartWith is not null);

                    customInput.RuleFor(x => x)
                        .Must(BeValidStringValidation)
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_StringValidation_Invalid)
                        .When(x => x.Type == TemplateCustomInputType.String);

                    customInput.RuleFor(x => x)
                        .Must(BeValidIntegerValidation)
                        .WithMessage(ErrorMessage.CreateTemplate_CustomInput_IntegerValidation_Invalid)
                        .When(x => x.Type == TemplateCustomInputType.Integer);
                });
        }

        private static bool HaveUniqueCustomInputNames(
            IReadOnlyCollection<CreateTemplateCustomInputCommandItem> customInputs)
        {
            var names = customInputs
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => x.Name.Trim().ToLowerInvariant())
                .ToArray();

            return names.Length == names.Distinct().Count();
        }

        private static bool HaveUniqueCustomInputOrders(
            IReadOnlyCollection<CreateTemplateCustomInputCommandItem> customInputs)
        {
            var orders = customInputs
                .Select(x => x.Order)
                .Where(x => x > 0)
                .ToArray();

            return orders.Length == orders.Distinct().Count();
        }

        private static bool BeValidStringValidation(CreateTemplateCustomInputCommandItem customInput)
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

        private static bool BeValidIntegerValidation(CreateTemplateCustomInputCommandItem customInput)
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
