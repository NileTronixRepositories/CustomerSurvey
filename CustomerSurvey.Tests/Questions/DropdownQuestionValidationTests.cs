using CustomerSurvey.Application.Features.GlobalQuestions.Command.CreateGlobalQuestion;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion;
using CustomerSurvey.Application.Features.Questions.Command.CreateQuestion;
using CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Enums;
using FluentValidation.Results;
using Xunit;

namespace CustomerSurvey.Tests.Questions;

public sealed class DropdownQuestionValidationTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(19)]
    public void Dropdown_options_are_valid_for_global_and_branch_create_and_update(int count)
    {
        var options = Enumerable.Range(1, count)
            .Select(i => new QuestionOptionCommandItem { TextEn = $"Option {i}", Order = i }).ToArray();

        Assert.All(Validate(options), result => Assert.True(result.IsValid, result.ToString()));
    }

    [Theory]
    [InlineData("empty", "Options")]
    [InlineData("duplicate order", "Options[1].Order")]
    [InlineData("duplicate English", "Options[1].TextEn")]
    [InlineData("duplicate Arabic", "Options[1].TextAr")]
    [InlineData("blank English", "Options[1].TextEn")]
    [InlineData("long English", "Options[1].TextEn")]
    [InlineData("long Arabic", "Options[1].TextAr")]
    [InlineData("zero order", "Options[1].Order")]
    [InlineData("negative order", "Options[1].Order")]
    [InlineData("nonzero value", "Options[1].Value")]
    public void Dropdown_options_must_have_valid_labels_order_and_neutral_value(string scenario, string property)
    {
        var first = new QuestionOptionCommandItem { TextEn = "First", TextAr = "أول", Order = 1 };
        var second = new QuestionOptionCommandItem { TextEn = "Second", Order = 2 };
        QuestionOptionCommandItem[] options = scenario switch
        {
            "empty" => [],
            "duplicate order" => [first, second with { Order = 1 }],
            "duplicate English" => [first, second with { TextEn = " FIRST " }],
            "duplicate Arabic" => [first, second with { TextAr = " أول " }],
            "blank English" => [first, second with { TextEn = "  " }],
            "long English" => [first, second with { TextEn = new string('a', 301) }],
            "long Arabic" => [first, second with { TextAr = new string('ب', 301) }],
            "zero order" => [first, second with { Order = 0 }],
            "negative order" => [first, second with { Order = -1 }],
            "nonzero value" => [first, second with { Value = 1 }],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        Assert.All(Validate(options), result => Assert.Contains(result.Errors, error => error.PropertyName == property));
    }

    private static ValidationResult[] Validate(QuestionOptionCommandItem[] options)
    {
        var groupId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        const QuestionType type = QuestionType.Dropdown;
        const string text = "Choose a service";

        return
        [
            new CreateQuestionCommandValidator().Validate(new CreateQuestionCommand
            {
                GroupId = groupId, Type = type, TextEn = text, Options = options
            }),
            new UpdateQuestionCommandValidator().Validate(new UpdateQuestionCommand
            {
                QuestionId = questionId, GroupId = groupId, Type = type, TextEn = text, Options = options
            }),
            new CreateGlobalQuestionCommandValidator().Validate(new CreateGlobalQuestionCommand
            {
                GroupId = groupId, Type = type, TextEn = text,
                Options = options.Select(o => new CreateGlobalQuestionOptionCommandItem
                {
                    TextEn = o.TextEn, TextAr = o.TextAr, Order = o.Order, Value = o.Value
                }).ToArray()
            }),
            new UpdateGlobalQuestionCommandValidator().Validate(new UpdateGlobalQuestionCommand
            {
                QuestionId = questionId, GroupId = groupId, Type = type, TextEn = text,
                Options = options.Select(o => new UpdateGlobalQuestionOptionCommandItem
                {
                    OptionId = o.OptionId, TextEn = o.TextEn, TextAr = o.TextAr, Order = o.Order, Value = o.Value
                }).ToArray()
            })
        ];
    }
}
