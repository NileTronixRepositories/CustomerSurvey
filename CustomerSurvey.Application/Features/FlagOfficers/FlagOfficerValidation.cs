using FluentValidation;

namespace CustomerSurvey.Application.Features.FlagOfficers;

internal sealed class FlagOfficerProfileValidator : AbstractValidator<FlagOfficerProfile>
{
    public FlagOfficerProfileValidator()
    {
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(200).EmailAddress();
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
    }
}

internal sealed class CreateFlagOfficerValidator : AbstractValidator<CreateFlagOfficerCommand>
{
    public CreateFlagOfficerValidator()
    {
        RuleFor(x => x.DepartmentId).Must(id => id != Guid.Empty);
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
        RuleFor(x => x.Profile).NotNull().SetValidator(new FlagOfficerProfileValidator());
    }
}

internal sealed class UpdateFlagOfficerValidator : AbstractValidator<UpdateFlagOfficerCommand>
{
    public UpdateFlagOfficerValidator()
    {
        RuleFor(x => x.Profile).NotNull().SetValidator(new FlagOfficerProfileValidator());
    }
}

internal sealed class AssignFlagOfficerRolesValidator : AbstractValidator<AssignFlagOfficerRolesCommand>
{
    public AssignFlagOfficerRolesValidator()
    {
        RuleFor(x => x.RoleIds).NotNull();
        RuleForEach(x => x.RoleIds).NotEmpty();
    }
}

internal sealed class GetFlagOfficersValidator : AbstractValidator<GetFlagOfficersQuery>
{
    public GetFlagOfficersValidator()
    {
        RuleFor(x => x.DepartmentId).Must(id => id != Guid.Empty);
        RuleFor(x => x.PageNumber).InclusiveBetween(1, int.MaxValue / 100);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.OrderSort).IsInEnum();
    }
}
