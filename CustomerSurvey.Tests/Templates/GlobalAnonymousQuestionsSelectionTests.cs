using System.Reflection;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class GlobalAnonymousQuestionsSelectionTests
{
    [Fact]
    public async Task Handle_AllowsNonArchivedGlobalBlueprintAndReturnsGlobalQuestionsOnly()
    {
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var template = CreateGlobalTemplate(userId);
        var globalGroup = QuestionGroup.CreateGlobal("Global", null, userId);
        var globalQuestion = Question.CreateGlobal(
            globalGroup.Id, "Global question", null, QuestionType.StarRating, userId);
        SetProperty(globalQuestion, nameof(Question.Group), globalGroup);

        var branchGroup = QuestionGroup.Create(branchId, "Branch", null, userId);
        var branchQuestion = Question.Create(
            branchId, branchGroup.Id, "Branch question", null, QuestionType.Smiles, userId);
        SetProperty(branchQuestion, nameof(Question.Group), branchGroup);

        var handler = CreateHandler(
            userId,
            branchId,
            template,
            new[] { globalQuestion, branchQuestion },
            isSuperAdmin: true);

        var result = await handler.Handle(new GetAnonymousTemplateQuestionsSelectionQuery
        {
            AnonymousTemplateId = template.Id
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var question = Assert.Single(result.Value.Questions);
        Assert.Equal(globalQuestion.Id, question.QuestionId);
        Assert.True(question.IsGlobal);
    }

    [Fact]
    public async Task Handle_RejectsArchivedGlobalBlueprint()
    {
        var userId = Guid.NewGuid();
        var template = CreateGlobalTemplate(userId);
        template.Deactivate();
        var handler = CreateHandler(
            userId,
            Guid.NewGuid(),
            template,
            Array.Empty<Question>(),
            isSuperAdmin: true);

        var result = await handler.Handle(new GetAnonymousTemplateQuestionsSelectionQuery
        {
            AnonymousTemplateId = template.Id
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error =>
            error.Code == "AnonymousTemplates.QuestionsSelection.TemplateArchived");
    }

    [Fact]
    public async Task Handle_StillRejectsInactiveBranchTemplate()
    {
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var template = AnonymousTemplate.CreateBranchTemplate(
            branchId, "Branch survey", null, null, DateTime.UtcNow, null, userId);
        template.Deactivate();
        var handler = CreateHandler(
            userId,
            branchId,
            template,
            Array.Empty<Question>(),
            isSuperAdmin: false);

        var result = await handler.Handle(new GetAnonymousTemplateQuestionsSelectionQuery
        {
            AnonymousTemplateId = template.Id
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error =>
            error.Code == "AnonymousTemplates.QuestionsSelection.TemplateInactive");
    }

    private static AnonymousTemplate CreateGlobalTemplate(Guid userId)
        => AnonymousTemplate.CreateGlobalTemplate(
            "Global blueprint", null, null, DateTime.UtcNow, null, userId);

    private static GetAnonymousTemplateQuestionsSelectionQueryHandler CreateHandler(
        Guid userId,
        Guid branchId,
        AnonymousTemplate template,
        IEnumerable<Question> questions,
        bool isSuperAdmin)
    {
        var templates = new InMemoryReadRepository<AnonymousTemplate>();
        templates.Add(template);
        var questionRepository = new InMemoryReadRepository<Question>();
        questionRepository.AddRange(questions);
        var superAdmins = new InMemoryReadRepository<SuperAdmin>();

        if (isSuperAdmin)
        {
            superAdmins.Add(SuperAdmin.Create(userId, userId));
        }

        return new GetAnonymousTemplateQuestionsSelectionQueryHandler(
            templates,
            new InMemoryReadRepository<AnonymousTemplateQuestion>(),
            questionRepository,
            new InMemoryReadRepository<QuestionOption>(),
            superAdmins,
            new TestBranchScopeResolver(branchId, userId),
            new TestCurrentUser(userId));
    }

    private static void SetProperty<TEntity, TValue>(
        TEntity entity,
        string propertyName,
        TValue value)
    {
        var property = typeof(TEntity).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(property);
        property!.SetValue(entity, value);
    }

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
}
