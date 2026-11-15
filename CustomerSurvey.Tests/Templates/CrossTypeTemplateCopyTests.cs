using System.Reflection;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.MultiTenancy;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.CopyAnonymousTemplateAsAuthorized;
using CustomerSurvey.Application.Features.Templates.Command.CopyTemplateAsAnonymous;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.infrastructure.Persistence;
using CustomerSurvey.infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class CrossTypeTemplateCopyTests
{
    [Fact]
    public async Task AuthorizedToAnonymous_ReusesQuestionsAndCopiesConditionsLogoAndCustomInputs()
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
            source.Id, "Phone", null, TemplateCustomInputType.String,
            true, 3, 20, null, null, "01", 1, userId));

        var firstLink = TemplateQuestion.Create(source.Id, Guid.NewGuid(), 1, userId, isRequired: false);
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
        var copiedInput = Assert.Single(target.CustomInputs, x => !x.SystemFieldKey.HasValue);
        var visitTime = Assert.Single(target.CustomInputs, x => x.SystemFieldKey == TemplateCustomInputSystemField.VisitTime);
        Assert.NotEqual(copiedInput.Order, visitTime.Order);
        Assert.False(visitTime.IsRequired);
        Assert.Equal("Phone", copiedInput.LabelEn);
        Assert.Null(copiedInput.LabelAr);
        Assert.Equal(TemplateCustomInputType.String, copiedInput.Type);
        Assert.True(copiedInput.IsRequired);
        Assert.Equal(3, copiedInput.MinLength);
        Assert.Equal(20, copiedInput.MaxLength);
        Assert.Equal("01", copiedInput.StartWith);
        Assert.Equal(1, copiedInput.Order);
        Assert.Equal("Media/TemplateLogos/copied.png", target.LogoPath);
        Assert.Equal($"https://survey.test/{target.Id}", target.PublicUrl);
        Assert.Equal($"qr::{target.PublicUrl}", target.QrCode);
        Assert.Equal(
            new[] { firstLink.QuestionId, secondLink.QuestionId },
            questionWrite.AddedEntities.OrderBy(x => x.Order).Select(x => x.QuestionId));
        Assert.Equal(new[] { false, true }, questionWrite.AddedEntities.OrderBy(x => x.Order).Select(x => x.IsRequired));

        var targetCondition = Assert.Single(conditionWrite.AddedEntities);
        Assert.NotEqual(sourceCondition.ParentTemplateQuestionId, targetCondition.ParentAnonymousTemplateQuestionId);
        Assert.NotEqual(sourceCondition.ChildTemplateQuestionId, targetCondition.ChildAnonymousTemplateQuestionId);
        Assert.Equal(5, targetCondition.TriggerValue);
        Assert.Equal("Media/TemplateLogos/source.png", media.LastCopiedPath);
    }

    [Fact]
    public async Task AnonymousToAuthorized_ReusesQuestionsAndCopiesConditionsLogoAndCustomInputs()
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
            source.Id, "Phone", null, TemplateCustomInputType.String,
            true, 3, 20, null, null, "01", 1, userId));

        var firstLink = AnonymousTemplateQuestion.Create(source.Id, Guid.NewGuid(), 1, userId, isRequired: false);
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
        var templateWrite = new InMemoryWriteRepository<Template>();
        var questionWrite = new InMemoryWriteRepository<TemplateQuestion>();
        var conditionWrite = new InMemoryWriteRepository<TemplateQuestionCondition>();
        var media = new CopyingMediaService();

        var handler = new CopyAnonymousTemplateAsAuthorizedCommandHandler(
            anonymousTemplates,
            templates,
            branches,
            conditions,
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
        var copiedInput = Assert.Single(target.CustomInputs, x => !x.SystemFieldKey.HasValue);
        var visitTime = Assert.Single(target.CustomInputs, x => x.SystemFieldKey == TemplateCustomInputSystemField.VisitTime);
        Assert.NotEqual(copiedInput.Order, visitTime.Order);
        Assert.False(visitTime.IsRequired);
        Assert.Equal("Phone", copiedInput.LabelEn);
        Assert.Null(copiedInput.LabelAr);
        Assert.Equal(TemplateCustomInputType.String, copiedInput.Type);
        Assert.True(copiedInput.IsRequired);
        Assert.Equal(3, copiedInput.MinLength);
        Assert.Equal(20, copiedInput.MaxLength);
        Assert.Equal("01", copiedInput.StartWith);
        Assert.Equal(1, copiedInput.Order);
        Assert.Equal("Media/TemplateLogos/copied.png", target.LogoPath);
        Assert.Equal(
            new[] { firstLink.QuestionId, secondLink.QuestionId },
            questionWrite.AddedEntities.OrderBy(x => x.Order).Select(x => x.QuestionId));
        Assert.Equal(new[] { false, true }, questionWrite.AddedEntities.OrderBy(x => x.Order).Select(x => x.IsRequired));

        var targetCondition = Assert.Single(conditionWrite.AddedEntities);
        Assert.NotEqual(sourceCondition.ParentAnonymousTemplateQuestionId, targetCondition.ParentTemplateQuestionId);
        Assert.NotEqual(sourceCondition.ChildAnonymousTemplateQuestionId, targetCondition.ChildTemplateQuestionId);
        Assert.Equal(4, targetCondition.TriggerValue);
        Assert.Equal("Media/TemplateLogos/source.png", media.LastCopiedPath);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AnonymousToAuthorized_AllowsManagedGlobalCopyAndReusesGlobalQuestions(bool hasExistingFamily)
    {
        var userId = Guid.NewGuid();
        var branch = Branch.Create("Cairo", null, "CAI", null, userId);
        var activeFrom = DateTime.UtcNow.AddDays(-1);
        var expireTo = activeFrom.AddDays(7);
        var catalog = AnonymousTemplate.CreateGlobalTemplate("Managed survey", null, "Description", activeFrom, expireTo, userId);
        Guid? existingFamilyId = hasExistingFamily ? Guid.NewGuid() : null;
        var source = AnonymousTemplate.CreateBranchTemplate(
            branch.Id, catalog.NameEn, catalog.NameAr, catalog.Description, activeFrom, expireTo, userId,
            templateFamilyId: existingFamilyId, sourceGlobalAnonymousTemplateId: catalog.Id);
        const string sourceLogo = "Media/TemplateLogos/managed.png";
        source.SetLogoPath(sourceLogo);

        var phoneInput = AnonymousTemplateCustomInput.Create(
            source.Id, "Phone", "الهاتف", TemplateCustomInputType.String,
            true, 3, 20, null, null, "01", 1, userId);
        var ageInput = AnonymousTemplateCustomInput.Create(
            source.Id, "Age", "العمر", TemplateCustomInputType.Integer,
            false, null, null, 18, 100, null, 3, userId);
        var inactiveInput = AnonymousTemplateCustomInput.Create(
            source.Id, "Old field", null, TemplateCustomInputType.String,
            false, null, null, null, null, null, 4, userId);
        inactiveInput.Deactivate();
        source.AddCustomInput(phoneInput);
        source.AddCustomInput(ageInput);
        source.AddCustomInput(inactiveInput);

        var group = QuestionGroup.CreateGlobal("Service", null, userId);
        var firstQuestion = Question.CreateGlobal(group.Id, "Was your request resolved?", null, QuestionType.SingleChoice, userId);
        var secondQuestion = Question.CreateGlobal(group.Id, "Tell us more", null, QuestionType.FreeText, userId);
        var option = QuestionOption.Create(firstQuestion.Id, "No", null, 1, 1, userId);
        var firstLink = AnonymousTemplateQuestion.Create(source.Id, firstQuestion.Id, 2, userId, isRequired: false);
        var secondLink = AnonymousTemplateQuestion.Create(source.Id, secondQuestion.Id, 5, userId);
        AddAnonymousTemplateQuestion(source, secondLink);
        AddAnonymousTemplateQuestion(source, firstLink);
        var condition = AnonymousTemplateQuestionCondition.CreateForSingleChoice(source.Id, firstLink.Id, secondLink.Id, option.Id, 3, userId);
        var inactiveCondition = AnonymousTemplateQuestionCondition.CreateForSingleChoice(source.Id, firstLink.Id, secondLink.Id, option.Id, 4, userId);
        inactiveCondition.Deactivate();

        var anonymousTemplates = new InMemoryReadRepository<AnonymousTemplate>();
        anonymousTemplates.Add(source);
        var branches = new InMemoryReadRepository<Branch>();
        branches.Add(branch);
        var conditions = new InMemoryReadRepository<AnonymousTemplateQuestionCondition>();
        conditions.AddRange([condition, inactiveCondition]);
        var media = new CopyingMediaService();
        var unitOfWork = new TestUnitOfWork();

        // Use the real write repositories and EF model to inspect pending writes without a database connection.
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>().UseSqlServer().Options;
        await using var db = new PlatformWriteDbContext(options, new CurrentTenantContext());
        db.AttachRange(branch, catalog, source, group, firstQuestion, secondQuestion, option, condition, inactiveCondition);
        var resolver = new PlatformWriteDbContextResolver(db);

        var handler = new CopyAnonymousTemplateAsAuthorizedCommandHandler(
            anonymousTemplates,
            new InMemoryReadRepository<Template>(),
            branches,
            conditions,
            new PlatformWriteRepository<Template>(resolver),
            new PlatformWriteRepository<TemplateQuestion>(resolver),
            new PlatformWriteRepository<TemplateQuestionCondition>(resolver),
            new TestBranchScopeResolver(branch.Id, userId),
            media,
            unitOfWork);

        var result = await handler.Handle(
            new CopyAnonymousTemplateAsAuthorizedCommand { AnonymousTemplateId = source.Id },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
        var target = Assert.Single(db.ChangeTracker.Entries<Template>(), x => x.State == EntityState.Added).Entity;
        Assert.NotEqual(source.Id, target.Id);
        Assert.Equal(target.Id, result.Value.TemplateId);
        Assert.Equal(branch.Id, target.BranchId);
        Assert.Equal(branch.Id, result.Value.BranchId);
        Assert.Equal(source.Id, result.Value.SourceAnonymousTemplateId);
        Assert.Equal(catalog.Id, source.SourceGlobalAnonymousTemplateId);
        Assert.Equal(result.Value.TemplateFamilyId, target.TemplateFamilyId);
        Assert.Equal(target.TemplateFamilyId, source.TemplateFamilyId);
        Assert.NotEqual(Guid.Empty, result.Value.TemplateFamilyId);
        Assert.Equal(existingFamilyId ?? result.Value.TemplateFamilyId, source.TemplateFamilyId);

        var sourceEntry = db.Entry(source);
        Assert.Equal(hasExistingFamily ? EntityState.Unchanged : EntityState.Modified, sourceEntry.State);
        var expectedSourceWrites = hasExistingFamily ? Array.Empty<string>() : new[] { nameof(AnonymousTemplate.TemplateFamilyId) };
        Assert.Equal(expectedSourceWrites, sourceEntry.Properties.Where(x => x.IsModified).Select(x => x.Metadata.Name));
        Assert.All(db.ChangeTracker.Entries().Where(x => x.Entity is QuestionGroup or Question or QuestionOption
            or AnonymousTemplateQuestion or AnonymousTemplateQuestionCondition or AnonymousTemplateCustomInput
            || ReferenceEquals(x.Entity, catalog)), x => Assert.Equal(EntityState.Unchanged, x.State));

        var targetLinks = db.ChangeTracker.Entries<TemplateQuestion>().Where(x => x.State == EntityState.Added)
            .Select(x => x.Entity).OrderBy(x => x.Order).ToArray();
        Assert.Equal(2, result.Value.QuestionsCount);
        Assert.Equal(new[] { (firstQuestion.Id, 2, false), (secondQuestion.Id, 5, true) },
            targetLinks.Select(x => (x.QuestionId, x.Order, x.IsRequired)));
        Assert.All(targetLinks, link =>
        {
            Assert.Equal(target.Id, link.TemplateId);
            Assert.Same(group, link.Question.Group);
        });

        Assert.Equal(1, result.Value.ConditionsCount);
        var targetCondition = Assert.Single(db.ChangeTracker.Entries<TemplateQuestionCondition>(), x => x.State == EntityState.Added).Entity;
        Assert.Equal(target.Id, targetCondition.TemplateId);
        Assert.Equal(targetLinks[0].Id, targetCondition.ParentTemplateQuestionId);
        Assert.Equal(targetLinks[1].Id, targetCondition.ChildTemplateQuestionId);
        Assert.Same(option, targetCondition.SelectedQuestionOption);
        Assert.Equal((condition.TriggerType, condition.TriggerValue, option.Id, condition.Order),
            (targetCondition.TriggerType, targetCondition.TriggerValue, targetCondition.SelectedQuestionOptionId, targetCondition.Order));

        Assert.Collection(target.CustomInputs.Where(x => !x.SystemFieldKey.HasValue).OrderBy(x => x.Order),
            input => AssertCopiedCustomInput(phoneInput, input),
            input => AssertCopiedCustomInput(ageInput, input));
        var visitTime = Assert.Single(target.CustomInputs, x => x.SystemFieldKey.HasValue);
        Assert.Equal(TemplateCustomInputSystemField.VisitTime, visitTime.SystemFieldKey);
        Assert.DoesNotContain(target.CustomInputs, input => input.Id != visitTime.Id && input.Order == visitTime.Order);
        Assert.Equal(sourceLogo, media.LastCopiedPath);
        Assert.Equal("Media/TemplateLogos/copied.png", target.LogoPath);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task AnonymousToAuthorized_RejectsSourceOutsideBranch(bool isGlobalSource)
    {
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var source = isGlobalSource
            ? AnonymousTemplate.CreateGlobalTemplate("Survey", null, null, DateTime.UtcNow, null, userId)
            : AnonymousTemplate.CreateBranchTemplate(Guid.NewGuid(), "Survey", null, null, DateTime.UtcNow, null, userId,
                sourceGlobalAnonymousTemplateId: Guid.NewGuid());

        return AssertAnonymousCopyRejected(source, branchId, userId, "SourceNotFound", ErrorType.NotFound);
    }

    [Fact]
    public Task AnonymousToAuthorized_RejectsInactiveManagedSource()
    {
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var source = AnonymousTemplate.CreateBranchTemplate(branchId, "Survey", null, null, DateTime.UtcNow, null, userId,
            sourceGlobalAnonymousTemplateId: Guid.NewGuid());
        source.Deactivate();

        return AssertAnonymousCopyRejected(source, branchId, userId, "SourceInactive", ErrorType.Validation);
    }

    [Fact]
    public Task AnonymousToAuthorized_RejectsNameConflictInBranch()
    {
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var source = AnonymousTemplate.CreateBranchTemplate(branchId, "Survey", null, null, DateTime.UtcNow, null, userId,
            sourceGlobalAnonymousTemplateId: Guid.NewGuid());
        var existing = Template.Create(branchId, source.NameEn, null, null, source.ActiveFrom, null, userId);

        return AssertAnonymousCopyRejected(source, branchId, userId, "NameConflict", ErrorType.Conflict, existing);
    }

    private static async Task AssertAnonymousCopyRejected(
        AnonymousTemplate source, Guid branchId, Guid userId, string errorCode, ErrorType errorType, Template? existing = null)
    {
        var anonymousTemplates = new InMemoryReadRepository<AnonymousTemplate>();
        anonymousTemplates.Add(source);
        var templates = new InMemoryReadRepository<Template>();
        if (existing is not null)
        {
            templates.Add(existing);
        }

        var templateWrite = new InMemoryWriteRepository<Template>();
        var unitOfWork = new TestUnitOfWork();
        var handler = new CopyAnonymousTemplateAsAuthorizedCommandHandler(
            anonymousTemplates,
            templates,
            new InMemoryReadRepository<Branch>(),
            new InMemoryReadRepository<AnonymousTemplateQuestionCondition>(),
            templateWrite,
            new InMemoryWriteRepository<TemplateQuestion>(),
            new InMemoryWriteRepository<TemplateQuestionCondition>(),
            new TestBranchScopeResolver(branchId, userId),
            new CopyingMediaService(),
            unitOfWork);

        var result = await handler.Handle(
            new CopyAnonymousTemplateAsAuthorizedCommand { AnonymousTemplateId = source.Id },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Errors);
        Assert.Equal($"AnonymousTemplates.CopyAsAuthorized.{errorCode}", error.Code);
        Assert.Equal(errorType, error.Type);
        Assert.Empty(templateWrite.AddedEntities);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
        Assert.Null(source.TemplateFamilyId);
    }

    private static void AssertCopiedCustomInput(AnonymousTemplateCustomInput source, TemplateCustomInput target)
    {
        Assert.True(target.IsActive);
        Assert.Equal((source.LabelEn, source.LabelAr, source.Type, source.IsRequired, source.Order),
            (target.LabelEn, target.LabelAr, target.Type, target.IsRequired, target.Order));
        Assert.Equal((source.MinLength, source.MaxLength, source.MinValue, source.MaxValue, source.StartWith),
            (target.MinLength, target.MaxLength, target.MinValue, target.MaxValue, target.StartWith));
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
