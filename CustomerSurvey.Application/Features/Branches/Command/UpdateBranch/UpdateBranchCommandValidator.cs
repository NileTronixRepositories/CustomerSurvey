using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.UpdateBranch
{
    internal sealed class UpdateBranchCommandValidator
         : AbstractValidator<UpdateBranchCommand>
    {
        public UpdateBranchCommandValidator()
        {
            RuleFor(x => x.BranchId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateBranch_BranchId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateBranch_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateBranch_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateBranch_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateBranch_Code_Required)
                .MaximumLength(50)
                .WithMessage(ErrorMessage.UpdateBranch_Code_MaxLength);

            RuleFor(x => x.Address)
                .MaximumLength(500)
                .WithMessage(ErrorMessage.UpdateBranch_Address_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.Address));
        }
    }
}