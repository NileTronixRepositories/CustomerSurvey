using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class TemplateSystemEnhancementDomainTests
{
    [Fact]
    public void AuthorizedTemplate_DeactivateAndRestore_ToggleIsActive()
    {
        var template = Template.Create(
            Guid.NewGuid(),
            "Authorized survey",
            null,
            null,
            DateTime.UtcNow,
            null,
            Guid.NewGuid());

        template.Deactivate();

        Assert.False(template.IsActive);

        template.Restore();

        Assert.True(template.IsActive);
    }

    [Fact]
    public void BranchAnonymousTemplate_DeactivateAndRestore_UseIsActiveOnly()
    {
        var template = AnonymousTemplate.CreateBranchTemplate(
            Guid.NewGuid(),
            "Anonymous survey",
            null,
            null,
            DateTime.UtcNow,
            null,
            Guid.NewGuid());

        template.Deactivate();

        Assert.False(template.IsActive);
        Assert.False(template.IsArchived);

        template.Restore();

        Assert.True(template.IsActive);
        Assert.False(template.IsArchived);
    }

    [Fact]
    public void GlobalAnonymousTemplate_Create_ProducesNonPublicBlueprint()
    {
        var template = AnonymousTemplate.CreateGlobalTemplate(
            "Global blueprint",
            null,
            null,
            DateTime.UtcNow,
            null,
            Guid.NewGuid());

        Assert.Equal(AnonymousTemplateScope.Global, template.Scope);
        Assert.Null(template.BranchId);
        Assert.False(template.IsActive);
        Assert.False(template.IsArchived);
        Assert.Null(template.PublicUrl);
        Assert.Null(template.QrCode);
        Assert.Null(template.LogoPath);
    }

    [Fact]
    public void GlobalAnonymousTemplate_DeactivateAndRestore_ToggleArchiveOnly()
    {
        var template = AnonymousTemplate.CreateGlobalTemplate(
            "Global blueprint",
            null,
            null,
            DateTime.UtcNow,
            null,
            Guid.NewGuid());

        template.Deactivate();

        Assert.True(template.IsArchived);
        Assert.False(template.IsActive);

        template.Restore();

        Assert.False(template.IsArchived);
        Assert.False(template.IsActive);
    }

    [Fact]
    public void ManagedGlobalBranchCopy_DeactivateAndRestore_UseBranchLifecycle()
    {
        var template = AnonymousTemplate.CreateBranchTemplate(
            Guid.NewGuid(),
            "Managed copy",
            null,
            null,
            DateTime.UtcNow,
            null,
            Guid.NewGuid(),
            sourceGlobalAnonymousTemplateId: Guid.NewGuid());

        template.Deactivate();

        Assert.False(template.IsActive);
        Assert.False(template.IsArchived);

        template.Restore();

        Assert.True(template.IsActive);
        Assert.False(template.IsArchived);
    }
}
