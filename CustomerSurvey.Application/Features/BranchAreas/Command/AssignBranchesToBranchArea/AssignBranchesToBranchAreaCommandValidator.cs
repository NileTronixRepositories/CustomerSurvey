using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.AssignBranchesToBranchArea
{
    internal sealed class AssignBranchesToBranchAreaCommandValidator
        : AbstractValidator<AssignBranchesToBranchAreaCommand>
    {
        public AssignBranchesToBranchAreaCommandValidator()
        {
            RuleFor(x => x.BranchAreaId)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignBranchAreaBranches_BranchArea_NotFound);

            RuleFor(x => x.BranchIds)
                .NotEmpty()
                .WithMessage(ErrorMessage.AssignBranchAreaBranches_Branches_Required)
                .Must(branchIds => branchIds.Distinct().Count() == branchIds.Count)
                .WithMessage(ErrorMessage.CreateBranchArea_BranchIds_Duplicated);

            RuleForEach(x => x.BranchIds)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranchArea_BranchIds_Invalid);
        }
    }
}
