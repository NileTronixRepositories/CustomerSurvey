using CustomerSurvey.Application.Features.GlobalQuestions.Command.CreateGlobalQuestion;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion;
using CustomerSurvey.Application.Features.Questions.Command.CreateQuestion;
using CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Enums;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class RatingQuestionValidationTests
{
    [Fact]
    public void Branch_rating_create_and_update_accept_the_fixed_scale_without_options()
    {
        var create = new CreateQuestionCommand
        {
            GroupId = Guid.NewGuid(), TextEn = "Recommendation", Type = QuestionType.Rating1To10
        };
        var update = new UpdateQuestionCommand
        {
            QuestionId = Guid.NewGuid(), GroupId = create.GroupId, TextEn = create.TextEn, Type = create.Type
        };
        QuestionOptionCommandItem[] options = [new() { TextEn = "Option", Order = 1, Value = 5 }];
        var createValidator = new CreateQuestionCommandValidator();
        var updateValidator = new UpdateQuestionCommandValidator();

        Assert.True(createValidator.Validate(create).IsValid);
        Assert.True(updateValidator.Validate(update).IsValid);
        Assert.Contains(createValidator.Validate(create with { Options = options }).Errors, x => x.PropertyName == "Options");
        Assert.Contains(updateValidator.Validate(update with { Options = options }).Errors, x => x.PropertyName == "Options");
    }

    [Fact]
    public void Global_rating_create_and_update_accept_the_fixed_scale_without_options()
    {
        var create = new CreateGlobalQuestionCommand
        {
            GroupId = Guid.NewGuid(), TextEn = "Recommendation", Type = QuestionType.Rating1To10
        };
        var update = new UpdateGlobalQuestionCommand
        {
            QuestionId = Guid.NewGuid(), GroupId = create.GroupId, TextEn = create.TextEn, Type = create.Type
        };
        var createValidator = new CreateGlobalQuestionCommandValidator();
        var updateValidator = new UpdateGlobalQuestionCommandValidator();

        Assert.True(createValidator.Validate(create).IsValid);
        Assert.True(updateValidator.Validate(update).IsValid);
        Assert.Contains(createValidator.Validate(create with { Options = [new() { TextEn = "Option", Order = 1, Value = 5 }] }).Errors,
            x => x.PropertyName == "Options");
        Assert.Contains(updateValidator.Validate(update with { Options = [new() { TextEn = "Option", Order = 1, Value = 5 }] }).Errors,
            x => x.PropertyName == "Options");
    }
}
