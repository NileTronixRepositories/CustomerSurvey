using CustomerSurvey.Application.Features.Templates.Query.GetSuperAdminTemplatesPagination;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class SuperAdminTemplateCatalogGlobalSourceTests
{
    [Fact]
    public async Task Handle_ReturnsGlobalAnonymousSourceWithNullableBranchMetadata()
    {
        var userId = Guid.NewGuid();
        var global = AnonymousTemplate.CreateGlobalTemplate(
            "Global blueprint", null, null, DateTime.UtcNow, null, userId);
        var context = CreateHandler(userId, global);

        var result = await context.Handler.Handle(new GetSuperAdminTemplatesPaginationQuery
        {
            TemplateKind = TemplateCatalogKind.Anonymous
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Data);
        Assert.Equal(global.Id, item.TemplateId);
        Assert.Null(item.BranchId);
        Assert.Null(item.BranchNameEn);
        Assert.Null(item.BranchNameAr);
        Assert.Equal(AnonymousTemplateScope.Global, item.Scope);
        Assert.True(item.IsGlobal);
        Assert.False(item.IsActive);
        Assert.False(item.IsArchived);
        Assert.Null(item.LogoPath);
        Assert.Null(item.SourceGlobalAnonymousTemplateId);
        Assert.False(item.IsManagedGlobalCopy);
    }

    [Fact]
    public async Task Handle_BranchFilterExcludesGlobalAnonymousSources()
    {
        var userId = Guid.NewGuid();
        var global = AnonymousTemplate.CreateGlobalTemplate(
            "Global blueprint", null, null, DateTime.UtcNow, null, userId);
        var branch = Branch.Create("Cairo", null, "CAI", null, userId);
        var context = CreateHandler(userId, global, branch);

        var result = await context.Handler.Handle(new GetSuperAdminTemplatesPaginationQuery
        {
            BranchId = branch.Id,
            TemplateKind = TemplateCatalogKind.Anonymous
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Data);
    }

    private static CatalogTestContext CreateHandler(
        Guid userId,
        AnonymousTemplate global,
        Branch? branch = null)
    {
        var anonymousTemplates = new InMemoryReadRepository<AnonymousTemplate>();
        anonymousTemplates.Add(global);
        var branches = new InMemoryReadRepository<Branch>();
        if (branch is not null)
        {
            branches.Add(branch);
        }

        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        superAdmins.Add(SuperAdmin.Create(userId, userId));

        var handler = new GetSuperAdminTemplatesPaginationQueryHandler(
            new InMemoryReadRepository<Template>(),
            anonymousTemplates,
            branches,
            superAdmins,
            new InMemoryReadRepository<ApplicationUser>(),
            new TestCurrentUser(userId));

        return new CatalogTestContext(handler);
    }

    private sealed record CatalogTestContext(
        GetSuperAdminTemplatesPaginationQueryHandler Handler);
}
