using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Auth.Command.SelectBranch
{
    internal sealed class SelectBranchCommandValidator : AbstractValidator<SelectBranchCommand>
    {
        public SelectBranchCommandValidator()
        {
            RuleFor(x => x.BranchId)
                .NotEmpty()
                .WithMessage(ErrorMessage.SelectBranch_BranchId_Required);
        }
    }
}
