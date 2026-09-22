using System.Reflection;
using BuildingBlock.Application.Abstraction.Media;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignGlobalAnonymousTemplateToBranch;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class AssignGlobalAnonymousTemplateToBranchTests
{
    [Fact]
    public async Task Handle_AssignsBlueprintWithExplicitDatesAndCopiesContent()
    {
        var userId = Guid.NewGuid();
        var source = CreateGlobalSource(userId);
        source.AddCustomInput(AnonymousTemplateCustomInput.Create(
            source.Id,
            "Phone",
            "Phone",
            null,
            TemplateCustomInputType.String,
            true,
            3,
            20,
            null,
            null,
            "01",
            1,
            userId));

        var firstSourceQuestion = AnonymousTemplateQuestion.Create(source.Id, Guid.NewGuid(), 1, userId);
        var secondSourceQuestion = AnonymousTemplateQuestion.Create(source.Id, Guid.NewGuid(), 2, userId);
        AddQuestion(source, firstSourceQuestion);
        AddQuestion(source, secondSourceQuestion);

        var sourceCondition = AnonymousTemplateQuestionCondition.CreateForStarRating(
            source.Id,
            firstSourceQuestion.Id,
            secondSourceQuestion.Id,
            4,
            1,
            userId);

        var branch = Branch.Create("Cairo", "القاهرة", "CAI", null, userId);
        var context = CreateContext(userId, source, branch, sourceCondition);
        var activeFrom = DateTime.UtcNow.AddDays(3);
        var expireTo = activeFrom.AddDays(30);
        var logo = CreatePngLogo();

        var result = await context.Handler.Handle(new AssignGlobalAnonymousTemplateToBranchCommand
        {
            GlobalTemplateId = source.Id,
            BranchId = branch.Id,
            ActiveFrom = activeFrom,
            ExpireTo = expireTo,
            Logo = logo
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var target = Assert.Single(context.TemplateWriteRepository.AddedEntities);
        Assert.NotEqual(source.Id, target.Id);
        Assert.Equal(source.Id, target.SourceGlobalAnonymousTemplateId);
        Assert.Equal(branch.Id, target.BranchId);
        Assert.Equal(activeFrom, target.ActiveFrom);
        Assert.Equal(expireTo, target.ExpireTo);
        Assert.True(target.IsActive);
        Assert.False(target.IsArchived);
        Assert.Equal("Media/TemplateLogos/saved.png", target.LogoPath);
        Assert.Equal($"https://survey.test/{target.Id}", target.PublicUrl);
        Assert.Equal($"qr::{target.PublicUrl}", target.QrCode);
        Assert.Single(target.CustomInputs);

        Assert.Equal(2, context.QuestionWriteRepository.AddedEntities.Count);
        Assert.Equal(
            new[] { firstSourceQuestion.QuestionId, secondSourceQuestion.QuestionId },
            context.QuestionWriteRepository.AddedEntities.OrderBy(x => x.Order).Select(x => x.QuestionId));

        var targetCondition = Assert.Single(context.ConditionWriteRepository.AddedEntities);
        Assert.NotEqual(sourceCondition.ParentAnonymousTemplateQuestionId, targetCondition.ParentAnonymousTemplateQuestionId);
        Assert.NotEqual(sourceCondition.ChildAnonymousTemplateQuestionId, targetCondition.ChildAnonymousTemplateQuestionId);
        Assert.Equal(4, targetCondition.TriggerValue);
        Assert.Equal(1, context.UnitOfWork.SaveChangesCount);
        Assert.Equal(1, context.MediaService.SaveCount);
    }

    [Fact]
    public async Task Handle_RejectsArchivedGlobalSource()
    {
        var userId = Guid.NewGuid();
        var source = CreateGlobalSource(userId);
        source.Deactivate();
        var branch = Branch.Create("Cairo", null, "CAI", null, userId);
        var context = CreateContext(userId, source, branch);

        var result = await context.Handler.Handle(CreateCommand(source.Id, branch.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "AnonymousTemplates.AssignGlobal.SourceArchived");
    }

    [Fact]
    public async Task Handle_RejectsInactiveTargetBranch()
    {
        var userId = Guid.NewGuid();
        var source = CreateGlobalSource(userId);
        var branch = Branch.Create("Cairo", null, "CAI", null, userId);
        branch.Deactivate();
        var context = CreateContext(userId, source, branch);

        var result = await context.Handler.Handle(CreateCommand(source.Id, branch.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "AnonymousTemplates.AssignGlobal.BranchInactive");
    }

    [Fact]
    public async Task Handle_RejectsDuplicateGlobalSourceAndBranchAssignment()
    {
        var userId = Guid.NewGuid();
        var source = CreateGlobalSource(userId);
        var branch = Branch.Create("Cairo", null, "CAI", null, userId);
        var context = CreateContext(userId, source, branch);
        context.TemplateReadRepository.Add(AnonymousTemplate.CreateBranchTemplate(
            branch.Id,
            source.NameEn,
            source.NameAr,
            source.Description,
            DateTime.UtcNow,
            null,
            userId,
            sourceGlobalAnonymousTemplateId: source.Id));

        var result = await context.Handler.Handle(CreateCommand(source.Id, branch.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "AnonymousTemplates.AssignGlobal.Duplicate");
    }

    private static AnonymousTemplate CreateGlobalSource(Guid userId)
        => AnonymousTemplate.CreateGlobalTemplate(
            "Global blueprint",
            "مخطط عالمي",
            "Description",
            DateTime.UtcNow.AddYears(-1),
            DateTime.UtcNow.AddYears(-1).AddDays(1),
            userId);

    private static AssignGlobalAnonymousTemplateToBranchCommand CreateCommand(
        Guid sourceId,
        Guid branchId)
    {
        var activeFrom = DateTime.UtcNow.AddDays(1);

        return new AssignGlobalAnonymousTemplateToBranchCommand
        {
            GlobalTemplateId = sourceId,
            BranchId = branchId,
            ActiveFrom = activeFrom,
            ExpireTo = activeFrom.AddDays(5)
        };
    }

    private static AssignmentTestContext CreateContext(
        Guid userId,
        AnonymousTemplate source,
        Branch branch,
        params AnonymousTemplateQuestionCondition[] conditions)
    {
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        superAdmins.Add(SuperAdmin.Create(userId, userId));

        var branches = new InMemoryReadRepository<Branch>();
        branches.Add(branch);

        var templates = new InMemoryReadRepository<AnonymousTemplate>();
        templates.Add(source);

        var conditionReadRepository = new InMemoryReadRepository<AnonymousTemplateQuestionCondition>();
        conditionReadRepository.AddRange(conditions);

        var templateWriteRepository = new InMemoryWriteRepository<AnonymousTemplate>();
        var questionWriteRepository = new InMemoryWriteRepository<AnonymousTemplateQuestion>();
        var conditionWriteRepository = new InMemoryWriteRepository<AnonymousTemplateQuestionCondition>();
        var mediaService = new TestMediaService();
        var unitOfWork = new TestUnitOfWork();

        var handler = new AssignGlobalAnonymousTemplateToBranchCommandHandler(
            superAdmins,
            branches,
            templates,
            conditionReadRepository,
            templateWriteRepository,
            questionWriteRepository,
            conditionWriteRepository,
            new TestPublicSurveyUrlBuilder(),
            new TestQrCodeGenerator(),
            mediaService,
            new TestCurrentUser(userId),
            unitOfWork);

        return new AssignmentTestContext(
            handler,
            templates,
            templateWriteRepository,
            questionWriteRepository,
            conditionWriteRepository,
            mediaService,
            unitOfWork);
    }

    private static void AddQuestion(
        AnonymousTemplate template,
        AnonymousTemplateQuestion question)
    {
        var field = typeof(AnonymousTemplate).GetField(
            "_questions",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(field);
        var questions = Assert.IsType<List<AnonymousTemplateQuestion>>(field!.GetValue(template));
        questions.Add(question);
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

    private sealed record AssignmentTestContext(
        AssignGlobalAnonymousTemplateToBranchCommandHandler Handler,
        InMemoryReadRepository<AnonymousTemplate> TemplateReadRepository,
        InMemoryWriteRepository<AnonymousTemplate> TemplateWriteRepository,
        InMemoryWriteRepository<AnonymousTemplateQuestion> QuestionWriteRepository,
        InMemoryWriteRepository<AnonymousTemplateQuestionCondition> ConditionWriteRepository,
        TestMediaService MediaService,
        TestUnitOfWork UnitOfWork);

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

        public Task<string> SaveAsync(IFormFile mediaFile, string folderName)
        {
            SaveCount++;
            return Task.FromResult("saved.png");
        }

        public Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName)
            => throw new NotSupportedException();

        public Task<string> CopyAsync(string relativePath, string folderName)
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
