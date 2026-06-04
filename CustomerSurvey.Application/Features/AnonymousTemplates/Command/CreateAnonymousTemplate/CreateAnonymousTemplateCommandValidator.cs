using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate
{
    internal sealed class CreateAnonymousTemplateCommandValidator
        : AbstractValidator<CreateAnonymousTemplateCommand>
    {
        private const int CustomInputStartWithMaxLength = 100;

        public CreateAnonymousTemplateCommandValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_Description_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.ActiveFrom)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_ActiveFrom_Required);

            RuleFor(x => x)
                .Must(x => !x.ExpireTo.HasValue || x.ExpireTo.Value > x.ActiveFrom)
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_ExpireTo_MustBeAfterActiveFrom);

            RuleFor(x => x.Scope)
                .Must(x => !x.HasValue ||
                           x.Value == AnonymousTemplateScope.Branch ||
                           x.Value == AnonymousTemplateScope.Global)
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_Scope_Invalid);

            RuleFor(x => x.CustomInputs)
                .Must(x => x is not null)
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInputs_Invalid);

            RuleForEach(x => x.CustomInputs)
                .ChildRules(input =>
                {
                    input.RuleFor(x => x.Name)
                        .NotEmpty()
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_Name_Required)
                        .MaximumLength(100)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_Name_MaxLength);

                    input.RuleFor(x => x.LabelEn)
                        .MaximumLength(200)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_LabelEn_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.LabelEn));

                    input.RuleFor(x => x.LabelAr)
                        .MaximumLength(200)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_LabelAr_MaxLength)
                        .When(x => !string.IsNullOrWhiteSpace(x.LabelAr));

                    input.RuleFor(x => x.Type)
                        .IsInEnum()
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_Type_Invalid);

                    input.RuleFor(x => x.Order)
                        .GreaterThan(0)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_Order_Invalid);

                    input.RuleFor(x => x)
                        .Must(x => x.Type == TemplateCustomInputType.String || x.StartWith is null)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_StartWith_NotAllowed);

                    input.RuleFor(x => x.StartWith)
                        .Must(x => !string.IsNullOrWhiteSpace(x))
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_StartWith_Empty)
                        .When(x => x.StartWith is not null);

                    input.RuleFor(x => x.StartWith)
                        .MaximumLength(CustomInputStartWithMaxLength)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_StartWith_MaxLength)
                        .When(x => x.StartWith is not null);

                    input.RuleFor(x => x)
                        .Must(ValidateStringValidationShape)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_StringValidation_Invalid);

                    input.RuleFor(x => x)
                        .Must(ValidateIntegerValidationShape)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_IntegerValidation_Invalid);

                    input.RuleFor(x => x)
                        .Must(x => !x.MinLength.HasValue || !x.MaxLength.HasValue || x.MaxLength.Value >= x.MinLength.Value)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_LengthRange_Invalid);

                    input.RuleFor(x => x)
                        .Must(x => !x.MinValue.HasValue || !x.MaxValue.HasValue || x.MaxValue.Value >= x.MinValue.Value)
                        .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_ValueRange_Invalid);
                });

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueInputNames)
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_Name_Duplicated);

            RuleFor(x => x.CustomInputs)
                .Must(HaveUniqueOrders)
                .WithMessage(ErrorMessage.CreateAnonymousTemplate_CustomInput_Order_Duplicated);
        }

        private static bool ValidateStringValidationShape(
            CreateAnonymousTemplateCustomInputCommandItem input)
        {
            if (input.Type != TemplateCustomInputType.String)
            {
                return true;
            }

            return !input.MinValue.HasValue && !input.MaxValue.HasValue;
        }

        private static bool ValidateIntegerValidationShape(
            CreateAnonymousTemplateCustomInputCommandItem input)
        {
            if (input.Type != TemplateCustomInputType.Integer)
            {
                return true;
            }

            return !input.MinLength.HasValue && !input.MaxLength.HasValue;
        }

        private static bool HaveUniqueInputNames(
            IReadOnlyCollection<CreateAnonymousTemplateCustomInputCommandItem> inputs)
        {
            var normalizedNames = inputs
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => x.Name.Trim().ToLowerInvariant())
                .ToArray();

            return normalizedNames.Length == normalizedNames.Distinct().Count();
        }

        private static bool HaveUniqueOrders(
            IReadOnlyCollection<CreateAnonymousTemplateCustomInputCommandItem> inputs)
        {
            var orders = inputs
                .Where(x => x.Order > 0)
                .Select(x => x.Order)
                .ToArray();

            return orders.Length == orders.Distinct().Count();
        }
    }
}
