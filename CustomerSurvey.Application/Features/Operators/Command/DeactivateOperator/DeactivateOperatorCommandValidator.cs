using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Operators.Command.DeactivateOperator
{
    internal sealed class DeactivateOperatorCommandValidator
        : AbstractValidator<DeactivateOperatorCommand>
    {
        public DeactivateOperatorCommandValidator()
        {
            RuleFor(x => x.OperatorId)
                .NotEmpty()
                .WithMessage(ErrorMessage.DeactivateOperator_OperatorId_Required);
        }
    }
}
