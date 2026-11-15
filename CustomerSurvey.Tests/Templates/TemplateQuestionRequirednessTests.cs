using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate;
using CustomerSurvey.Domain.Entities;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class TemplateQuestionRequirednessTests
{
    [Fact]
    public void Assignment_factories_default_to_required()
    {
        var templateId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        Assert.True(TemplateQuestion.Create(templateId, questionId, 1, actorId).IsRequired);
        Assert.True(AnonymousTemplateQuestion.Create(templateId, questionId, 1, actorId).IsRequired);
    }

    [Fact]
    public void Authorized_assignment_rejects_duplicate_empty_and_unrelated_requirements()
    {
        var id = Guid.NewGuid();
        var request = new AssignQuestionsToTemplateCommand { TemplateId = Guid.NewGuid(), QuestionIds = [id] };
        var validator = new AssignQuestionsToTemplateCommandValidator();
        Assert.True(validator.Validate(request).IsValid);
        Assert.True(validator.Validate(request with { QuestionRequirements = [new(id, false)] }).IsValid);
        Assert.False(validator.Validate(request with { QuestionRequirements = [new(id, true), new(id, false)] }).IsValid);
        Assert.False(validator.Validate(request with { QuestionRequirements = [new(Guid.Empty, false)] }).IsValid);
        Assert.False(validator.Validate(request with { QuestionRequirements = [new(Guid.NewGuid(), false)] }).IsValid);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(0, true)]
    [InlineData(225, true)]
    [InlineData(int.MaxValue, true)]
    [InlineData(-1, false)]
    public void Operator_submission_duration_is_optional_and_nonnegative(int? duration, bool expectedValid)
    {
        var validation = new SubmitOperatorTemplateResponseCommandValidator().Validate(
            new SubmitOperatorTemplateResponseCommand { TemplateId = Guid.NewGuid(), AnswerDurationSeconds = duration });

        Assert.Equal(expectedValid, validation.IsValid);
        if (!expectedValid)
        {
            Assert.Equal("AnswerDurationSeconds", Assert.Single(validation.Errors).PropertyName);
        }
    }

    [Fact]
    public void Submission_validators_accept_an_empty_answer_collection()
    {
        Assert.True(new SubmitOperatorTemplateResponseCommandValidator().Validate(
            new SubmitOperatorTemplateResponseCommand { TemplateId = Guid.NewGuid(), Answers = [] }).IsValid);
        Assert.True(new SubmitAnonymousTemplateResponseCommandValidator().Validate(
            new SubmitAnonymousTemplateResponseCommand { AnonymousTemplateId = Guid.NewGuid(), Answers = [] }).IsValid);
    }
}
