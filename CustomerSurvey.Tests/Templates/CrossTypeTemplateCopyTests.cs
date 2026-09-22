using System.Reflection;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.CopyAnonymousTemplateAsAuthorized;
using CustomerSurvey.Application.Features.Templates.Command.CopyTemplateAsAnonymous;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class CrossTypeTemplateCopyTests
{
    [Fact]
    public async Task AuthorizedToAnonymous_ReusesQuestionsCopiesConditionsAndLogoWithoutCustomInputs()
    {
        var userId = Guid.NewGuid();
        var branch = Branch.Create("Cairo", null, "CAI", null, userId);
        var source = Template.Create(
            branch.Id,
            "Customer survey",
            null,
            "Description",
            DateTime.UtcNow,
            null,
            userId);
        source.SetLogoPath("Media/TemplateLogos/source.png");
        source.AddCustomInput(TemplateCustomInput.Create(
            source.Id, "Phone", null, null, TemplateCustomInputType.String,
            true, 3, 20, null, null, "01", 1, userId));

        var firstLink = TemplateQuestion.Create(source.Id, Guid.NewGuid(), 1, userId);
        var secondLink = TemplateQuestion.Create(source.Id, Guid.NewGuid(), 2, userId);
        AddTemplateQuestion(source, firstLink);
        AddTemplateQuestion(source, secondLink);
        var sourceCondition = TemplateQuestionCondition.CreateForStarRating(
            source.Id, firstLink.Id, secondLink.Id, 5, 1, userId);

        var templates = new InMemoryReadRepository<Template>();
        templates.Add(source);
        var anonymousTemplates = new InMemoryReadRepository<AnonymousTemplate>();
        var branches = new InMemoryReadRepository<Branch>();
        branches.Add(branch);
        var conditions = new InMemoryReadRepository<TemplateQuestionCondition>();
        conditions.Add(sourceCondition);
        var templateWrite = new InMemoryWriteRepository<Template>();
        var anonymousWrite = new InMemoryWriteRepository<AnonymousTemplate>();
        var questionWrite = new InMemoryWriteRepository<AnonymousTemplateQuestion>();
        var conditionWrite = new InMemoryWriteRepository<AnonymousTemplateQuestionCondition>();
        var media = new CopyingMediaService();

        var handler = new CopyTemplateAsAnonymousCommandHandler(
            templates,
            anonymousTemplates,
            branches,
            conditions,
            templateWrite,
            anonymousWrite,
            questionWrite,
            conditionWrite,
            new TestBranchScopeResolver(branch.Id, userId),
            new TestPublicSurveyUrlBuilder(),
            new TestQrCodeGenerator(),
            media,
            new TestUnitOfWork());

        var result = await handler.Handle(
            new CopyTemplateAsAnonymousCommand { TemplateId = source.Id },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var target = Assert.Single(anonymousWrite.AddedEntities);
        Assert.NotEqual(source.Id, target.Id);
        Assert.Equal(source.TemplateFamilyId, target.TemplateFamilyId);
        Assert.NotNull(target.TemplateFamilyId);
        Assert.Empty(target.CustomInputs);
        Assert.Equal("Media/TemplateLogos/copied.png", target.LogoPath);
        Assert.Equal($"https://survey.test/{target.Id}", target.PublicUrl);
        Assert.Equal($"qr::{target.PublicUrl}", target.QrCode);
        Assert.Equal(
            new[] { firstLink.QuestionId, secondLink.QuestionId },
            questionWrite.AddedEntities.OrderBy(x => x.Order).Select(x => x.QuestionId));

        var targetCondition = Assert.Single(conditionWrite.AddedEntities);
        Assert.NotEqual(sourceCondition.ParentTemplateQuestionId, targetCondition.ParentAnonymousTemplateQuestionId);
        Assert.NotEqual(sourceCondition.ChildTemplateQuestionId, targetCondition.ChildAnonymousTemplateQuestionId);
        Assert.Equal(5, targetCondition.TriggerValue);
        Assert.Equal("Media/TemplateLogos/source.png", media.LastCopiedPath);
    }

    [Fact]
    public async Task AnonymousToAuthorized_ReusesQuestionsCopiesConditionsAndLogoWithoutCustomInputs()
    {
        var userId = Guid.NewGuid();
        var branch = Branch.Create("Cairo", null, "CAI", null, userId);
        var source = AnonymousTemplate.CreateBranchTemplate(
            branch.Id,
            "Customer survey",
            null,
            "Description",
            DateTime.UtcNow,
            null,
            userId);
        source.SetLogoPath("Media/TemplateLogos/source.png");
        source.AddCustomInput(AnonymousTemplateCustomInput.Create(
            source.Id, "Phone", null, null, TemplateCustomInputType.String,
            true, 3, 20, null, null, "01", 1, userId));

        var firstLink = AnonymousTemplateQuestion.Create(source.Id, Guid.NewGuid(), 1, userId);
        var secondLink = AnonymousTemplateQuestion.Create(source.Id, Guid.NewGuid(), 2, userId);
        AddAnonymousTemplateQuestion(source, firstLink);
        AddAnonymousTemplateQuestion(source, secondLink);
        var sourceCondition = AnonymousTemplateQuestionCondition.CreateForSmiles(
            source.Id, firstLink.Id, secondLink.Id, 4, 1, userId);

        var anonymousTemplates = new InMemoryReadRepository<AnonymousTemplate>();
        anonymousTemplates.Add(source);
        var templates = new InMemoryReadRepository<Template>();
        var branches = new InMemoryReadRepository<Branch>();
        branches.Add(branch);
        var conditions = new InMemoryReadRepository<AnonymousTemplateQuestionCondition>();
        conditions.Add(sourceCondition);
        var anonymousWrite = new InMemoryWriteRepository<AnonymousTemplate>();
        var templateWrite = new InMemoryWriteRepository<Template>();
        var questionWrite = new InMemoryWriteRepository<TemplateQuestion>();
        var conditionWrite = new InMemoryWriteRepository<TemplateQuestionCondition>();
        var media = new CopyingMediaService();

        var handler = new CopyAnonymousTemplateAsAuthorizedCommandHandler(
            anonymousTemplates,
            templates,
            branches,
            conditions,
            anonymousWrite,
            templateWrite,
            questionWrite,
            conditionWrite,
            new TestBranchScopeResolver(branch.Id, userId),
            media,
            new TestUnitOfWork());

        var result = await handler.Handle(
            new CopyAnonymousTemplateAsAuthorizedCommand { AnonymousTemplateId = source.Id },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var target = Assert.Single(templateWrite.AddedEntities);
        Assert.NotEqual(source.Id, target.Id);
        Assert.Equal(source.TemplateFamilyId, target.TemplateFamilyId);
        Assert.NotNull(target.TemplateFamilyId);
        Assert.Empty(target.CustomInputs);
        Assert.Equal("Media/TemplateLogos/copied.png", target.LogoPath);
        Assert.Equal(
            new[] { firstLink.QuestionId, secondLink.QuestionId },
            questionWrite.AddedEntities.OrderBy(x => x.Order).Select(x => x.QuestionId));

        var targetCondition = Assert.Single(conditionWrite.AddedEntities);
        Assert.NotEqual(sourceCondition.ParentAnonymousTemplateQuestionId, targetCondition.ParentTemplateQuestionId);
        Assert.NotEqual(sourceCondition.ChildAnonymousTemplateQuestionId, targetCondition.ChildTemplateQuestionId);
        Assert.Equal(4, targetCondition.TriggerValue);
        Assert.Equal("Media/TemplateLogos/source.png", media.LastCopiedPath);
    }

    [Fact]
    public async Task AnonymousToAuthorized_RejectsManagedGlobalCopy()
    {
        var userId = Guid.NewGuid();
        var branch = Branch.Create("Cairo", null, "CAI", null, userId);
        var source = AnonymousTemplate.CreateBranchTemplate(
            branch.Id,
            "Managed survey",
            null,
            null,
            DateTime.UtcNow,
            null,
            userId,
            sourceGlobalAnonymousTemplateId: Guid.NewGuid());
        var anonymousTemplates = new InMemoryReadRepository<AnonymousTemplate>();
        anonymousTemplates.Add(source);
        var branches = new InMemoryReadRepository<Branch>();
        branches.Add(branch);

        var handler = new CopyAnonymousTemplateAsAuthorizedCommandHandler(
            anonymousTemplates,
            new InMemoryReadRepository<Template>(),
            branches,
            new InMemoryReadRepository<AnonymousTemplateQuestionCondition>(),
            new InMemoryWriteRepository<AnonymousTemplate>(),
            new InMemoryWriteRepository<Template>(),
            new InMemoryWriteRepository<TemplateQuestion>(),
            new InMemoryWriteRepository<TemplateQuestionCondition>(),
            new TestBranchScopeResolver(branch.Id, userId),
            new CopyingMediaService(),
            new TestUnitOfWork());

        var result = await handler.Handle(
            new CopyAnonymousTemplateAsAuthorizedCommand { AnonymousTemplateId = source.Id },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "AnonymousTemplates.CopyAsAuthorized.ManagedCopyForbidden");
    }

    private static void AddTemplateQuestion(Template template, TemplateQuestion question)
        => GetList<Template, TemplateQuestion>(template, "_templateQuestions").Add(question);

    private static void AddAnonymousTemplateQuestion(
        AnonymousTemplate template,
        AnonymousTemplateQuestion question)
        => GetList<AnonymousTemplate, AnonymousTemplateQuestion>(template, "_questions").Add(question);

    private static List<TItem> GetList<TEntity, TItem>(TEntity entity, string fieldName)
    {
        var field = typeof(TEntity).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<List<TItem>>(field!.GetValue(entity));
    }

    private sealed class TestBranchScopeResolver(Guid branchId, Guid userId)
        : ICurrentBranchScopeResolver
    {
        public Task<Result<CurrentBranchScope>> ResolveAsync(CancellationToken cancellationToken = default)
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

    private sealed class CopyingMediaService : IMediaService
    {
        public string? LastCopiedPath { get; private set; }

        public Task<string> CopyAsync(string relativePath, string folderName)
        {
            LastCopiedPath = relativePath;
            return Task.FromResult("copied.png");
        }

        public Task<string> SaveAsync(IFormFile mediaFile, string folderName)
            => throw new NotSupportedException();

        public Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName)
            => throw new NotSupportedException();

        public Task<Stream> GetStream(IFormFile formFile)
            => throw new NotSupportedException();

        public void Remove(string filePath)
        {
        }

        public void RemoveRange(IEnumerable<string> filePaths)
        {
        }

        public Task<string> SaveVideoAsync(IFormFile videoFile, string folderName)
            => throw new NotSupportedException();
    }
}
