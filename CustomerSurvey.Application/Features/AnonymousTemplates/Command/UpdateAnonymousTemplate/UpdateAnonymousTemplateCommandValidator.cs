using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate
{
    internal sealed class UpdateAnonymousTemplateCommandValidator
        : AbstractValidator<UpdateAnonymousTemplateCommand>
    {
        private const int CustomInputStartWithMaxLength = 100;

        public UpdateAnonymousTemplateCommandValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_AnonymousTemplateId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_Description_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.ActiveFrom)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_ActiveFrom_Required);

            RuleFor(x => x)
                .Must(x => !x.ExpireTo.HasValue || x.ExpireTo.Value > x.ActiveFrom)
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_ExpireTo_MustBeAfterActiveFrom);

            RuleFor(x => x.CustomInputs)
                .NotNull()
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInputs_Invalid);

            RuleForEach(x => x.CustomInputs)
                .ChildRules(input =>
                {
                    input.RuleFor(x => x.Name)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_Name_Required)
                        .MaximumLength(100)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_Name_MaxLength);

                    input.RuleFor(x => x.LabelEn)
                        .MaximumLength(200)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_LabelEn_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.LabelEn));

                    input.RuleFor(x => x.LabelAr)
                        .MaximumLength(200)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_LabelAr_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.LabelAr));

                    input.RuleFor(x => x.Type)
                        .IsInEnum()
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_Type_Invalid);

                    input.RuleFor(x => x.Order)
                        .GreaterThan(0)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_Order_Invalid);

                    input.RuleFor(x => x)
                        .Must(x => x.Type == TemplateCustomInputType.String || x.StartWith is null)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_StartWith_NotAllowed);

                    input.RuleFor(x => x.StartWith)
                        .Must(x => !string.IsNullOrWhiteSpace(x))
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_StartWith_Empty)
                        .When(x => x.StartWith is not null);

                    input.RuleFor(x => x.StartWith)
                        .MaximumLength(CustomInputStartWithMaxLength)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_StartWith_MaxLength)
                        .When(x => x.StartWith is not null);

                    input.RuleFor(x => x)
                        .Must(ValidateStringValidationShape)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_StringValidation_Invalid);

                    input.RuleFor(x => x)
                        .Must(ValidateIntegerValidationShape)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_IntegerValidation_Invalid);

                    input.RuleFor(x => x)
                        .Must(x => !x.MinLength.HasValue || !x.MaxLength.HasValue || x.MaxLength.Value >= x.MinLength.Value)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_LengthRange_Invalid);

                    input.RuleFor(x => x)
                        .Must(x => !x.MinValue.HasValue || !x.MaxValue.HasValue || x.MaxValue.Value >= x.MinValue.Value)
                        .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_ValueRange_Invalid);
                });

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueInputIds)
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_Id_Duplicated);

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueInputNames)
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_Name_Duplicated);

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueOrders)
                .WithMessage(ErrorMessage.UpdateAnonymousTemplate_CustomInput_Order_Duplicated);
        }

        private static bool ValidateStringValidationShape(
            UpdateAnonymousTemplateCustomInputCommandItem input)
        {
            if (input.Type != TemplateCustomInputType.String)
            {
                return true;
            }

            return !input.MinValue.HasValue && !input.MaxValue.HasValue;
        }

        private static bool ValidateIntegerValidationShape(
            UpdateAnonymousTemplateCustomInputCommandItem input)
        {
            if (input.Type != TemplateCustomInputType.Integer)
            {
                return true;
            }

            return !input.MinLength.HasValue && !input.MaxLength.HasValue;
        }

        private static bool HaveUniqueInputIds(
            IReadOnlyCollection<UpdateAnonymousTemplateCustomInputCommandItem> inputs)
        {
            var ids = inputs
                .Where(x => x.CustomInputId.HasValue && x.CustomInputId.Value != Guid.Empty)
                .Select(x => x.CustomInputId!.Value)
                .ToArray();

            return ids.Length == ids.Distinct().Count();
        }

        private static bool HaveUniqueInputNames(
            IReadOnlyCollection<UpdateAnonymousTemplateCustomInputCommandItem> inputs)
        {
            var normalizedNames = inputs
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => x.Name.Trim().ToLowerInvariant())
                .ToArray();

            return normalizedNames.Length == normalizedNames.Distinct().Count();
        }

        private static bool HaveUniqueOrders(
            IReadOnlyCollection<UpdateAnonymousTemplateCustomInputCommandItem> inputs)
        {
            var orders = inputs
                .Where(x => x.Order > 0)
                .Select(x => x.Order)
                .ToArray();

            return orders.Length == orders.Distinct().Count();
        }
    }
}
