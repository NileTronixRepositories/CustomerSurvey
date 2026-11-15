using BuildingBlock.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion;
using CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Application.Shared.BranchScope;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Xunit;
using SecurityUserType = BuildingBlock.Application.Abstraction.Security.UserType;

namespace CustomerSurvey.Tests.Questions;

public sealed class QuestionUpdateValidationTests
{
    [Fact]
    public async Task Branch_question_update_enforces_authenticated_scope_and_group_ownership()
    {
        var actorId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var otherBranchId = Guid.NewGuid();
        var group = QuestionGroup.Create(branchId, "Group", null, actorId);
        var inactiveGroup = QuestionGroup.Create(branchId, "Inactive group", null, actorId);
        inactiveGroup.Deactivate();
        var otherGroup = QuestionGroup.Create(otherBranchId, "Other group's branch", null, actorId);
        var globalGroup = QuestionGroup.CreateGlobal("Global", null, actorId);
        var ownedQuestion = Question.Create(branchId, group.Id, "Owned", null, QuestionType.FreeText, actorId);
        var otherQuestion = Question.Create(otherBranchId, otherGroup.Id, "Other", null, QuestionType.FreeText, actorId);
        var globalQuestion = Question.CreateGlobal(globalGroup.Id, "Global", null, QuestionType.FreeText, actorId);
        var questions = new InMemoryReadRepository<Question>();
        questions.AddRange([ownedQuestion, otherQuestion, globalQuestion]);
        var groups = new InMemoryReadRepository<QuestionGroup>();
        groups.AddRange([group, inactiveGroup, otherGroup, globalGroup]);
        var branchAdmins = new InMemoryReadRepository<BranchAdmin>();
        branchAdmins.Add(BranchAdmin.Create(actorId, branchId, actorId));
        var unit = new TestUnitOfWork();
        var questionWrites = new InMemoryWriteRepository<Question>();
        var updater = new QuestionDefinitionUpdater(questionWrites, new InMemoryReadRepository<QuestionOption>(),
            new InMemoryWriteRepository<QuestionOption>(), new InMemoryReadRepository<TemplateQuestionCondition>(),
            new InMemoryReadRepository<AnonymousTemplateQuestionCondition>(), unit);
        var ownedUpdate = new UpdateQuestionCommand
        {
            QuestionId = ownedQuestion.Id, GroupId = group.Id, Type = QuestionType.StarRating, TextEn = "Updated"
        };
        var handler = Handler(new TestCurrentUser(actorId));

        await AssertRejectedAsync(Handler(new TestCurrentUser(null, false)), ownedUpdate, "Questions.Update.Unauthenticated");
        await AssertRejectedAsync(Handler(new TestCurrentUser(Guid.NewGuid())), ownedUpdate, "CurrentBranchScope.CurrentBranchActorNotFound");
        await AssertRejectedAsync(Handler(new BranchAreaWithoutBranch(Guid.NewGuid())), ownedUpdate, "BranchArea.SelectedBranchRequired");
        await AssertRejectedAsync(handler, ownedUpdate with { QuestionId = otherQuestion.Id }, "Questions.Update.QuestionNotFound");
        await AssertRejectedAsync(handler, ownedUpdate with { QuestionId = globalQuestion.Id }, "Questions.Update.QuestionNotFound");
        await AssertRejectedAsync(handler, ownedUpdate with { GroupId = otherGroup.Id }, "Questions.Update.QuestionGroupNotFound");
        await AssertRejectedAsync(handler, ownedUpdate with { GroupId = inactiveGroup.Id }, "Questions.Update.QuestionGroupInactive");
        Assert.Empty(questionWrites.UpdatedEntities);
        Assert.Equal(0, unit.SaveChangesCount);

        var textUpdate = await handler.Handle(ownedUpdate with { Type = ownedQuestion.Type }, default);
        Assert.True(textUpdate.IsSuccess);
        Assert.Equal(QuestionType.FreeText, textUpdate.Value.Type);
        Assert.Equal("Updated", textUpdate.Value.TextEn);
        Assert.Equal(1, unit.SaveChangesCount);

        UpdateQuestionCommandHandler Handler(ICurrentUser user) => new(questions, groups, updater,
            new CurrentBranchScopeResolver(user, branchAdmins, new InMemoryReadRepository<BranchUser>(),
                new InMemoryReadRepository<BranchArea>(), new InMemoryReadRepository<BranchAreaBranch>()), user);

        static async Task AssertRejectedAsync(UpdateQuestionCommandHandler handler, UpdateQuestionCommand request, string code)
        {
            var result = await handler.Handle(request, default);
            Assert.Equal(code, Assert.Single(result.Errors).Code);
        }
    }

    [Fact]
    public void Global_and_branch_question_updates_still_validate_option_values()
    {
        var questionId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var branch = new UpdateQuestionCommandValidator().Validate(new UpdateQuestionCommand
        {
            QuestionId = questionId, GroupId = groupId, Type = QuestionType.SingleChoice, TextEn = "Question",
            Options = [
                new() { TextEn = "First", Order = 1, Value = 1 },
                new() { TextEn = "Second", Order = 2, Value = 6 }]
        });
        var global = new UpdateGlobalQuestionCommandValidator().Validate(new UpdateGlobalQuestionCommand
        {
            QuestionId = questionId, GroupId = groupId, Type = QuestionType.SingleChoice, TextEn = "Question",
            Options = [new() { TextEn = "First", Order = 1, Value = 6 }]
        });

        Assert.Contains(branch.Errors, x => x.ErrorMessage == ErrorMessage.UpdateQuestion_Option_Value_Invalid);
        Assert.Contains(global.Errors, x => x.ErrorMessage == ErrorMessage.UpdateGlobalQuestion_Option_Value_Invalid);
    }

    private sealed class BranchAreaWithoutBranch(Guid id) : ICurrentUser
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => id;
        public Guid? AccountId => null;
        public Guid? ActiveBranchId => null;
        public string? Role => null;
        public SecurityUserType UserType => SecurityUserType.Unknown;
        public int? UserTypeValue => (int)Domain.Enums.UserType.BranchArea;
    }
}
