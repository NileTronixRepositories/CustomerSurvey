using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplateLogo;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.RestoreAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplateLogo;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class ManagedGlobalCopyRestrictionTests
{
    [Fact]
    public async Task BranchActor_CannotUpdateManagedCopyContent()
    {
        var context = CreateContext(isSuperAdmin: false);
        var handler = new UpdateAnonymousTemplateCommandHandler(
            context.Templates,
            context.TemplateWrites,
            new InMemoryReadRepository<Branch>(),
            new InMemoryReadRepository<AnonymousTemplateCustomInput>(),
            new InMemoryWriteRepository<AnonymousTemplateCustomInput>(),
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            context.UnitOfWork,
            new TestPublicSurveyUrlBuilder(),
            new TestQrCodeGenerator());

        var result = await handler.Handle(new UpdateAnonymousTemplateCommand
        {
            AnonymousTemplateId = context.Template.Id,
            NameEn = "Changed by branch",
            NameAr = context.Template.NameAr,
            Description = context.Template.Description,
            ActiveFrom = context.Template.ActiveFrom,
            ExpireTo = context.Template.ExpireTo
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, x =>
            x.Code == "AnonymousTemplates.Update.ManagedCopyReadOnly" &&
            x.Type == ErrorType.Security);
        Assert.Empty(context.TemplateWrites.UpdatedEntities);
    }

    [Fact]
    public async Task BranchActor_CannotAssignQuestionsOrManageConditions()
    {
        var context = CreateContext(isSuperAdmin: false);
        var assignHandler = CreateAssignQuestionsHandler(context);
        var conditionsHandler = CreateConditionsHandler(context);

        var assignResult = await assignHandler.Handle(
            new AssignQuestionsToAnonymousTemplateCommand
            {
                AnonymousTemplateId = context.Template.Id
            },
            CancellationToken.None);

        var conditionsResult = await conditionsHandler.Handle(
            new ManageAnonymousTemplateQuestionConditionsCommand
            {
                AnonymousTemplateId = context.Template.Id
            },
            CancellationToken.None);

        Assert.True(assignResult.IsFailure);
        Assert.Contains(assignResult.Errors, x =>
            x.Code == "AnonymousTemplates.AssignQuestions.ManagedCopyReadOnly");
        Assert.True(conditionsResult.IsFailure);
        Assert.Contains(conditionsResult.Errors, x =>
            x.Code == "AnonymousTemplates.ManageConditions.ManagedCopyReadOnly");
    }

    [Fact]
    public async Task BranchActor_CannotUpdateOrDeleteManagedCopyLogo()
    {
        var context = CreateContext(isSuperAdmin: false);
        context.Template.SetLogoPath("Media/TemplateLogos/existing.png");
        var media = new TestMediaService();
        var updateHandler = new UpdateAnonymousTemplateLogoCommandHandler(
            context.Templates,
            context.TemplateWrites,
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            media,
            context.UnitOfWork);
        var deleteHandler = new DeleteAnonymousTemplateLogoCommandHandler(
            context.Templates,
            context.TemplateWrites,
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            media,
            context.UnitOfWork);

        var updateResult = await updateHandler.Handle(
            new UpdateAnonymousTemplateLogoCommand
            {
                AnonymousTemplateId = context.Template.Id,
                Logo = CreatePngLogo()
            },
            CancellationToken.None);
        var deleteResult = await deleteHandler.Handle(
            new DeleteAnonymousTemplateLogoCommand
            {
                AnonymousTemplateId = context.Template.Id
            },
            CancellationToken.None);

        Assert.True(updateResult.IsFailure);
        Assert.Contains(updateResult.Errors, x =>
            x.Code == "AnonymousTemplates.Logo.ManagedCopyForbidden");
        Assert.True(deleteResult.IsFailure);
        Assert.Contains(deleteResult.Errors, x =>
            x.Code == "AnonymousTemplates.Logo.ManagedCopyForbidden");
        Assert.Equal("Media/TemplateLogos/existing.png", context.Template.LogoPath);
        Assert.Equal(0, media.SaveCount);
        Assert.Equal(0, media.RemoveCount);
    }

    [Fact]
    public async Task BranchActor_CanDeactivateAndRestoreManagedCopy()
    {
        var context = CreateContext(isSuperAdmin: false);
        var deleteHandler = new DeleteAnonymousTemplateCommandHandler(
            context.Templates,
            context.TemplateWrites,
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            context.UnitOfWork);
        var restoreHandler = new RestoreAnonymousTemplateCommandHandler(
            context.Templates,
            context.TemplateWrites,
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            context.UnitOfWork);

        var deleteResult = await deleteHandler.Handle(
            new DeleteAnonymousTemplateCommand
            {
                AnonymousTemplateId = context.Template.Id
            },
            CancellationToken.None);

        Assert.True(deleteResult.IsSuccess);
        Assert.False(context.Template.IsActive);

        var restoreResult = await restoreHandler.Handle(
            new RestoreAnonymousTemplateCommand
            {
                AnonymousTemplateId = context.Template.Id
            },
            CancellationToken.None);

        Assert.True(restoreResult.IsSuccess);
        Assert.True(context.Template.IsActive);
        Assert.Equal(2, context.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task SuperAdmin_CanUpdateScheduleButCannotChangeProtectedContent()
    {
        var context = CreateContext(isSuperAdmin: true);
        var handler = new UpdateAnonymousTemplateCommandHandler(
            context.Templates,
            context.TemplateWrites,
            new InMemoryReadRepository<Branch>(),
            new InMemoryReadRepository<AnonymousTemplateCustomInput>(),
            new InMemoryWriteRepository<AnonymousTemplateCustomInput>(),
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            context.UnitOfWork,
            new TestPublicSurveyUrlBuilder(),
            new TestQrCodeGenerator());
        var newActiveFrom = context.Template.ActiveFrom.AddDays(2);
        var newExpireTo = context.Template.ExpireTo!.Value.AddDays(2);

        var scheduleResult = await handler.Handle(new UpdateAnonymousTemplateCommand
        {
            AnonymousTemplateId = context.Template.Id,
            NameEn = context.Template.NameEn,
            NameAr = context.Template.NameAr,
            Description = context.Template.Description,
            ActiveFrom = newActiveFrom,
            ExpireTo = newExpireTo
        }, CancellationToken.None);

        Assert.True(scheduleResult.IsSuccess);
        Assert.Equal(newActiveFrom, context.Template.ActiveFrom);
        Assert.Equal(newExpireTo, context.Template.ExpireTo);

        var protectedContentResult = await handler.Handle(new UpdateAnonymousTemplateCommand
        {
            AnonymousTemplateId = context.Template.Id,
            NameEn = "Changed by SuperAdmin",
            NameAr = context.Template.NameAr,
            Description = context.Template.Description,
            ActiveFrom = context.Template.ActiveFrom,
            ExpireTo = context.Template.ExpireTo
        }, CancellationToken.None);

        Assert.True(protectedContentResult.IsFailure);
        Assert.Contains(protectedContentResult.Errors, x =>
            x.Code == "AnonymousTemplates.Update.ManagedCopyProtectedFields");
    }

    [Fact]
    public async Task SuperAdmin_CanUpdateAndDeleteManagedCopyLogo()
    {
        var context = CreateContext(isSuperAdmin: true);
        context.Template.SetLogoPath("Media/TemplateLogos/existing.png");
        var media = new TestMediaService();
        var updateHandler = new UpdateAnonymousTemplateLogoCommandHandler(
            context.Templates,
            context.TemplateWrites,
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            media,
            context.UnitOfWork);
        var deleteHandler = new DeleteAnonymousTemplateLogoCommandHandler(
            context.Templates,
            context.TemplateWrites,
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            media,
            context.UnitOfWork);

        var updateResult = await updateHandler.Handle(
            new UpdateAnonymousTemplateLogoCommand
            {
                AnonymousTemplateId = context.Template.Id,
                Logo = CreatePngLogo()
            },
            CancellationToken.None);
        var deleteResult = await deleteHandler.Handle(
            new DeleteAnonymousTemplateLogoCommand
            {
                AnonymousTemplateId = context.Template.Id
            },
            CancellationToken.None);

        Assert.True(updateResult.IsSuccess);
        Assert.True(deleteResult.IsSuccess);
        Assert.Null(context.Template.LogoPath);
        Assert.Equal(1, media.SaveCount);
        Assert.Equal(2, media.RemoveCount);
    }

    [Fact]
    public async Task SuperAdmin_CannotAssignQuestionsOrManageConditionsOnManagedCopy()
    {
        var context = CreateContext(isSuperAdmin: true);

        var assignResult = await CreateAssignQuestionsHandler(context).Handle(
            new AssignQuestionsToAnonymousTemplateCommand
            {
                AnonymousTemplateId = context.Template.Id
            },
            CancellationToken.None);
        var conditionsResult = await CreateConditionsHandler(context).Handle(
            new ManageAnonymousTemplateQuestionConditionsCommand
            {
                AnonymousTemplateId = context.Template.Id
            },
            CancellationToken.None);

        Assert.True(assignResult.IsFailure);
        Assert.Contains(assignResult.Errors, x =>
            x.Code == "AnonymousTemplates.AssignQuestions.TemplateNotFound");
        Assert.True(conditionsResult.IsFailure);
        Assert.Contains(conditionsResult.Errors, x =>
            x.Code == "AnonymousTemplates.ManageConditions.TemplateNotFound");
    }

    private static AssignQuestionsToAnonymousTemplateCommandHandler CreateAssignQuestionsHandler(
        ManagedCopyTestContext context)
        => new(
            context.Templates,
            new InMemoryReadRepository<AnonymousTemplateQuestion>(),
            new InMemoryWriteRepository<AnonymousTemplateQuestion>(),
            new InMemoryReadRepository<AnonymousTemplateQuestionCondition>(),
            new InMemoryWriteRepository<AnonymousTemplateQuestionCondition>(),
            new InMemoryReadRepository<Question>(),
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            context.UnitOfWork);

    private static ManageAnonymousTemplateQuestionConditionsCommandHandler CreateConditionsHandler(
        ManagedCopyTestContext context)
        => new(
            context.Templates,
            new InMemoryReadRepository<AnonymousTemplateQuestion>(),
            new InMemoryReadRepository<AnonymousTemplateQuestionCondition>(),
            new InMemoryWriteRepository<AnonymousTemplateQuestionCondition>(),
            new InMemoryReadRepository<QuestionOption>(),
            context.SuperAdmins,
            context.ScopeResolver,
            context.CurrentUser,
            context.UnitOfWork);

    private static ManagedCopyTestContext CreateContext(bool isSuperAdmin)
    {
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var template = AnonymousTemplate.CreateBranchTemplate(
            branchId,
            "Managed copy",
            "نسخة مُدارة",
            "Protected content",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30),
            userId,
            sourceGlobalAnonymousTemplateId: Guid.NewGuid());
        var templates = new InMemoryReadRepository<AnonymousTemplate>();
        templates.Add(template);
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        if (isSuperAdmin)
        {
            superAdmins.Add(SuperAdmin.Create(userId, userId));
        }

        return new ManagedCopyTestContext(
            template,
            templates,
            new InMemoryWriteRepository<AnonymousTemplate>(),
            superAdmins,
            new TestBranchScopeResolver(branchId, userId),
            new TestCurrentUser(userId),
            new TestUnitOfWork());
    }

    private static FormFile CreatePngLogo()
    {
        byte[] content = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, stream.Length, "Logo", "logo.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
    }

    private sealed record ManagedCopyTestContext(
        AnonymousTemplate Template,
        InMemoryReadRepository<AnonymousTemplate> Templates,
        InMemoryWriteRepository<AnonymousTemplate> TemplateWrites,
        InMemoryReadRepository<SuperAdmin> SuperAdmins,
        TestBranchScopeResolver ScopeResolver,
        TestCurrentUser CurrentUser,
        TestUnitOfWork UnitOfWork);

    private sealed class TestBranchScopeResolver(Guid branchId, Guid userId)
        : ICurrentBranchScopeResolver
    {
        public Task<Result<CurrentBranchScope>> ResolveAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result<CurrentBranchScope>.Ok(new CurrentBranchScope
            {
                ApplicationUserId = userId,
                BranchId = branchId,
                ActorType = UserType.BranchAdmin,
                ActorProfileId = Guid.NewGuid()
            }));
    }

    private sealed class TestPublicSurveyUrlBuilder : IPublicSurveyUrlBuilder
    {
        public string BuildAnonymousTemplateUrl(Guid anonymousTemplateId)
            => $"https://survey.test/{anonymousTemplateId}";
    }

    private sealed class TestQrCodeGenerator : IQrCodeGenerator
    {
        public string GenerateBase64Png(string text) => $"qr::{text}";
    }

    private sealed class TestMediaService : IMediaService
    {
        public int SaveCount { get; private set; }
        public int RemoveCount { get; private set; }

        public Task<string> SaveAsync(IFormFile mediaFile, string folderName)
        {
            SaveCount++;
            return Task.FromResult("managed-copy.png");
        }

        public Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName)
            => throw new NotSupportedException();

        public Task<string> CopyAsync(string relativePath, string folderName)
            => throw new NotSupportedException();

        public Task<Stream> GetStream(IFormFile formFile)
            => throw new NotSupportedException();

        public void Remove(string filePath) => RemoveCount++;

        public void RemoveRange(IEnumerable<string> filePaths)
            => RemoveCount += filePaths.Count();

        public Task<string> SaveVideoAsync(IFormFile videoFile, string folderName)
            => throw new NotSupportedException();
    }
}
