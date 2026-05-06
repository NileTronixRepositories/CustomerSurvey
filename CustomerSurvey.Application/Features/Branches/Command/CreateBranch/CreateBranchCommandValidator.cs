using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranch
{
    internal sealed class CreateBranchCommandValidator
      : AbstractValidator<CreateBranchCommand>
    {
        public CreateBranchCommandValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranch_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranch_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.CreateBranch_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateBranch_Code_Required)
                .MaximumLength(50)
                .WithMessage(ErrorMessage.CreateBranch_Code_MaxLength);

            RuleFor(x => x.Address)
                .MaximumLength(500)
                .WithMessage(ErrorMessage.CreateBranch_Address_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.Address));
        }
    }
}