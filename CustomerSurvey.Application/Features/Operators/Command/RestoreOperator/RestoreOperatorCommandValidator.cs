using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Operators.Command.RestoreOperator
{
    internal sealed class RestoreOperatorCommandValidator
        : AbstractValidator<RestoreOperatorCommand>
    {
        public RestoreOperatorCommandValidator()
        {
            RuleFor(x => x.OperatorId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreOperator_OperatorId_Required);
        }
    }
}
