using System.Reflection;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class CopyTemplateToBranchEnhancementTests
{
    [Fact]
    public async Task Handle_ReusesGlobalQuestionAndCopiesCustomInputWithoutLogoOrFamily()
    {
        var context = CreateContext();
        var group = QuestionGroup.CreateGlobal("Global", null, context.UserId);
        var question = Question.CreateGlobal(
            group.Id, "Rate us", null, QuestionType.StarRating, context.UserId);
        SetProperty(question, nameof(Question.Group), group);
        var source = CreateTemplate(context, "Source");
        source.SetLogoPath("Media/TemplateLogos/source.png");
        source.SetTemplateFamily(Guid.NewGuid());
        var input = TemplateCustomInput.Create(
            source.Id, "Phone", "Phone", null, TemplateCustomInputType.String,
            true, 3, 20, null, null, "01", 1, context.UserId);
        source.AddCustomInput(input);
        AddTemplateQuestion(source, question, 1, context.UserId);
        context.Templates.Add(source);

        var result = await context.CopyAsync(source.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(context.GroupWrites.AddedEntities);
        Assert.Empty(context.QuestionWrites.AddedEntities);
        Assert.Empty(context.OptionWrites.AddedEntities);
        var targetLink = Assert.Single(context.TemplateQuestionWrites.AddedEntities);
        Assert.Equal(question.Id, targetLink.QuestionId);

        var target = Assert.Single(context.TemplateWrites.AddedEntities);
        Assert.NotEqual(source.Id, target.Id);
        Assert.Equal(context.TargetBranch.Id, target.BranchId);
        Assert.Null(target.LogoPath);
        Assert.Null(target.TemplateFamilyId);
        Assert.Equal(source.Id, target.OriginTemplateId);
        var copiedInput = Assert.Single(target.CustomInputs);
        Assert.NotEqual(input.Id, copiedInput.Id);
        Assert.Equal(input.Name, copiedInput.Name);
        Assert.Equal(input.StartWith, copiedInput.StartWith);
        Assert.Equal(input.MinLength, copiedInput.MinLength);
        Assert.Equal(input.MaxLength, copiedInput.MaxLength);
    }

    [Fact]
    public async Task Handle_LazilyClonesOnlyUsedBranchQuestionAndItsOptions()
    {
        var context = CreateContext();
        var group = QuestionGroup.Create(
            context.SourceBranch.Id, "Service", "الخدمة", context.UserId);
        var q1 = CreateBranchQuestion(context, group, "Q1", QuestionType.SingleChoice);
        var q2 = CreateBranchQuestion(context, group, "Q2", QuestionType.Complain);
        var q3 = CreateBranchQuestion(context, group, "Q3", QuestionType.Voice);
        var o1 = AddOption(q1, "Good", "جيد", 1, 5, context.UserId);
        var o2 = AddOption(q1, "Bad", "سيئ", 2, 1, context.UserId);
        var source = CreateTemplate(context, "Lazy source");
        AddTemplateQuestion(source, q1, 1, context.UserId);
        context.Templates.Add(source);

        var result = await context.CopyAsync(source.Id);

        Assert.True(result.IsSuccess);
        var copiedGroup = Assert.Single(context.GroupWrites.AddedEntities);
        Assert.Equal(group.Id, copiedGroup.OriginQuestionGroupId);
        var copiedQuestion = Assert.Single(context.QuestionWrites.AddedEntities);
        Assert.Equal(q1.Id, copiedQuestion.OriginQuestionId);
        Assert.DoesNotContain(context.QuestionWrites.AddedEntities, x =>
            x.OriginQuestionId == q2.Id || x.OriginQuestionId == q3.Id);

        Assert.Equal(2, context.OptionWrites.AddedEntities.Count);
        AssertOptionCopied(o1, context.OptionWrites.AddedEntities.Single(x =>
            x.OriginQuestionOptionId == o1.Id));
        AssertOptionCopied(o2, context.OptionWrites.AddedEntities.Single(x =>
            x.OriginQuestionOptionId == o2.Id));
    }

    [Fact]
    public async Task Handle_ReusesExistingGroupQuestionAndOptionAndRemapsCondition()
    {
        var context = CreateContext();
        var sourceGroup = QuestionGroup.Create(
            context.SourceBranch.Id, "Service", null, context.UserId);
        var q1 = CreateBranchQuestion(context, sourceGroup, "Q1", QuestionType.SingleChoice);
        var q2 = CreateBranchQuestion(context, sourceGroup, "Q2", QuestionType.Complain);
        var sourceOption = AddOption(q1, "Continue", null, 1, 5, context.UserId);

        var targetGroup = QuestionGroup.Create(
            context.TargetBranch.Id, sourceGroup.NameEn, sourceGroup.NameAr,
            context.UserId, sourceGroup.Id);
        var targetQ1 = Question.Create(
            context.TargetBranch.Id, targetGroup.Id, q1.TextEn, q1.TextAr,
            q1.Type, context.UserId, q1.Id);
        var targetOption = QuestionOption.Create(
            targetQ1.Id, sourceOption.TextEn, sourceOption.TextAr,
            sourceOption.Order, sourceOption.Value, context.UserId, sourceOption.Id);
        context.Groups.Add(targetGroup);
        context.Questions.Add(targetQ1);
        context.Options.Add(targetOption);

        var source = CreateTemplate(context, "Reuse source");
        var firstLink = AddTemplateQuestion(source, q1, 1, context.UserId);
        var secondLink = AddTemplateQuestion(source, q2, 2, context.UserId);
        context.Templates.Add(source);
        context.TemplateConditions.Add(TemplateQuestionCondition.CreateForSingleChoice(
            source.Id, firstLink.Id, secondLink.Id, sourceOption.Id, 1, context.UserId));

        var result = await context.CopyAsync(source.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(context.GroupWrites.AddedEntities);
        Assert.Single(context.QuestionWrites.AddedEntities);
        Assert.Equal(q2.Id, context.QuestionWrites.AddedEntities[0].OriginQuestionId);
        Assert.Empty(context.OptionWrites.AddedEntities);
        Assert.Contains(context.TemplateQuestionWrites.AddedEntities, x => x.QuestionId == targetQ1.Id);

        var copiedCondition = Assert.Single(context.TemplateConditionWrites.AddedEntities);
        Assert.Equal(targetOption.Id, copiedCondition.SelectedQuestionOptionId);
        Assert.NotEqual(sourceOption.Id, copiedCondition.SelectedQuestionOptionId);
    }

    [Fact]
    public async Task Handle_RejectsDuplicateRootOrigin()
    {
        var context = CreateContext();
        var source = CreateTemplate(context, "Source");
        context.Templates.Add(source);
        context.Templates.Add(Template.Create(
            context.TargetBranch.Id, "Existing copy", null, null,
            DateTime.UtcNow, null, context.UserId, originTemplateId: source.Id));

        var result = await context.CopyAsync(source.Id);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error =>
            error.Code == "Templates.CopyToBranch.DuplicateOrigin");
    }

    [Fact]
    public async Task Handle_RejectsInactiveSource()
    {
        var context = CreateContext();
        var source = CreateTemplate(context, "Source");
        source.Deactivate();
        context.Templates.Add(source);

        var result = await context.CopyAsync(source.Id);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error =>
            error.Code == "Templates.CopyToBranch.SourceInactive");
    }

    [Fact]
    public async Task Handle_ReturnsValidationForExistingInactiveTargetBranch()
    {
        var context = CreateContext(targetBranchActive: false);
        var source = CreateTemplate(context, "Source");
        context.Templates.Add(source);

        var result = await context.CopyAsync(source.Id);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error =>
            error.Code == "Templates.CopyToBranch.BranchInactive" &&
            error.Type == BuildingBlock.Domain.Results.ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ReturnsNotFoundForMissingTargetBranch()
    {
        var context = CreateContext(addTargetBranch: false);
        var source = CreateTemplate(context, "Source");
        context.Templates.Add(source);

        var result = await context.CopyAsync(source.Id);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error =>
            error.Code == "Templates.CopyToBranch.BranchNotFound" &&
            error.Type == BuildingBlock.Domain.Results.ErrorType.NotFound);
    }

    private static CopyContext CreateContext(
        bool targetBranchActive = true,
        bool addTargetBranch = true)
    {
        var userId = Guid.NewGuid();
        var sourceBranch = Branch.Create("Source", null, "SRC", null, userId);
        var targetBranch = Branch.Create("Target", null, "TGT", null, userId);
        if (!targetBranchActive)
        {
            targetBranch.Deactivate();
        }

        var superAdmins = new InMemoryReadRepository<SuperAdmin>();
        superAdmins.Add(SuperAdmin.Create(userId, userId));
        var branches = new InMemoryReadRepository<Branch>();
        branches.Add(sourceBranch);
        if (addTargetBranch)
        {
            branches.Add(targetBranch);
        }

        var templates = new InMemoryReadRepository<Template>();
        var anonymousTemplates = new InMemoryReadRepository<AnonymousTemplate>();
        var templateConditions = new InMemoryReadRepository<TemplateQuestionCondition>();
        var anonymousConditions = new InMemoryReadRepository<AnonymousTemplateQuestionCondition>();
        var groups = new InMemoryReadRepository<QuestionGroup>();
        var questions = new InMemoryReadRepository<Question>();
        var options = new InMemoryReadRepository<QuestionOption>();
        var templateWrites = new InMemoryWriteRepository<Template>();
        var templateQuestionWrites = new InMemoryWriteRepository<TemplateQuestion>();
        var templateConditionWrites = new InMemoryWriteRepository<TemplateQuestionCondition>();
        var groupWrites = new InMemoryWriteRepository<QuestionGroup>();
        var questionWrites = new InMemoryWriteRepository<Question>();
        var optionWrites = new InMemoryWriteRepository<QuestionOption>();

        var handler = new CopyTemplateToBranchCommandHandler(
            superAdmins,
            branches,
            templates,
            anonymousTemplates,
            templateConditions,
            anonymousConditions,
            groups,
            questions,
            options,
            templateWrites,
            templateQuestionWrites,
            templateConditionWrites,
            new InMemoryWriteRepository<AnonymousTemplate>(),
            new InMemoryWriteRepository<AnonymousTemplateQuestion>(),
            new InMemoryWriteRepository<AnonymousTemplateQuestionCondition>(),
            groupWrites,
            questionWrites,
            optionWrites,
            new TestPublicSurveyUrlBuilder(),
            new TestQrCodeGenerator(),
            new TestCurrentUser(userId),
            new TestUnitOfWork());

        return new CopyContext(
            handler, userId, sourceBranch, targetBranch,
            templates, templateConditions, groups, questions, options,
            templateWrites, templateQuestionWrites, templateConditionWrites,
            groupWrites, questionWrites, optionWrites);
    }

    private static Template CreateTemplate(CopyContext context, string name)
        => Template.Create(
            context.SourceBranch.Id, name, null, null,
            DateTime.UtcNow, null, context.UserId);

    private static Question CreateBranchQuestion(
        CopyContext context,
        QuestionGroup group,
        string text,
        QuestionType type)
    {
        var question = Question.Create(
            context.SourceBranch.Id, group.Id, text, null, type, context.UserId);
        SetProperty(question, nameof(Question.Group), group);
        return question;
    }

    private static TemplateQuestion AddTemplateQuestion(
        Template template,
        Question question,
        int order,
        Guid userId)
    {
        var link = TemplateQuestion.Create(template.Id, question.Id, order, userId);
        SetProperty(link, nameof(TemplateQuestion.Question), question);
        GetList<Template, TemplateQuestion>(template, "_templateQuestions").Add(link);
        return link;
    }

    private static QuestionOption AddOption(
        Question question,
        string textEn,
        string? textAr,
        int order,
        int value,
        Guid userId)
    {
        var option = QuestionOption.Create(
            question.Id, textEn, textAr, order, value, userId);
        GetList<Question, QuestionOption>(question, "_options").Add(option);
        return option;
    }

    private static void AssertOptionCopied(QuestionOption source, QuestionOption target)
    {
        Assert.NotEqual(source.Id, target.Id);
        Assert.Equal(source.TextEn, target.TextEn);
        Assert.Equal(source.TextAr, target.TextAr);
        Assert.Equal(source.Order, target.Order);
        Assert.Equal(source.Value, target.Value);
        Assert.Equal(source.Id, target.OriginQuestionOptionId);
    }

    private static List<TItem> GetList<TEntity, TItem>(TEntity entity, string fieldName)
    {
        var field = typeof(TEntity).GetField(
            fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<List<TItem>>(field!.GetValue(entity));
    }

    private static void SetProperty<TEntity, TValue>(
        TEntity entity,
        string propertyName,
        TValue value)
    {
        var property = typeof(TEntity).GetProperty(
            propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(property);
        property!.SetValue(entity, value);
    }

    private sealed record CopyContext(
        CopyTemplateToBranchCommandHandler Handler,
        Guid UserId,
        Branch SourceBranch,
        Branch TargetBranch,
        InMemoryReadRepository<Template> Templates,
        InMemoryReadRepository<TemplateQuestionCondition> TemplateConditions,
        InMemoryReadRepository<QuestionGroup> Groups,
        InMemoryReadRepository<Question> Questions,
        InMemoryReadRepository<QuestionOption> Options,
        InMemoryWriteRepository<Template> TemplateWrites,
        InMemoryWriteRepository<TemplateQuestion> TemplateQuestionWrites,
        InMemoryWriteRepository<TemplateQuestionCondition> TemplateConditionWrites,
        InMemoryWriteRepository<QuestionGroup> GroupWrites,
        InMemoryWriteRepository<Question> QuestionWrites,
        InMemoryWriteRepository<QuestionOption> OptionWrites)
    {
        public Task<BuildingBlock.Domain.Results.Result<CopyTemplateToBranchResponse>> CopyAsync(
            Guid sourceTemplateId)
            => Handler.Handle(new CopyTemplateToBranchCommand
            {
                TemplateId = sourceTemplateId,
                BranchId = TargetBranch.Id
            }, CancellationToken.None);
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
}
