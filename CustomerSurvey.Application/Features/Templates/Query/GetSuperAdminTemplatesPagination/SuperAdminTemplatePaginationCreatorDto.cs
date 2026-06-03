namespace CustomerSurvey.Application.Features.Templates.Query.GetSuperAdminTemplatesPagination;

internal sealed record SuperAdminTemplatePaginationCreatorDto
{
    public Guid ApplicationUserId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }
}
