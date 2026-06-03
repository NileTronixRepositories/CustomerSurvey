using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Templates.Query.GetSuperAdminTemplatesPagination;

internal sealed class GetSuperAdminTemplatesPaginationQueryValidator
    : AbstractValidator<GetSuperAdminTemplatesPaginationQuery>
{
    public GetSuperAdminTemplatesPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.GetTemplatesPagination_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.GetTemplatesPagination_PageSize_Invalid)
            .LessThanOrEqualTo(100)
            .WithMessage(ErrorMessage.GetTemplatesPagination_PageSize_Max);

        RuleFor(x => x.TemplateKind)
            .IsInEnum()
            .When(x => x.TemplateKind.HasValue)
            .WithMessage(ErrorMessage.GetSuperAdminTemplatesPagination_TemplateKind_Invalid);
    }
}
