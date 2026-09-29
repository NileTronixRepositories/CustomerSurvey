using System.Reflection;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Application.Features.Templates.Command.CreateTemplate;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Xunit;

namespace CustomerSurvey.Tests.SurveyResponses;

public sealed class FreeTextAndCustomInputLabelTests
{
    [Fact]
    public void FreeText_UsesStableEnumValueAndFactoriesPersistTrimmedText()
    {
        Assert.Equal(7, (int)QuestionType.FreeText);

        var responseId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var operatorAnswer = SurveyAnswer.CreateFreeText(responseId, questionId, "  Excellent service  ");
        var anonymousAnswer = AnonymousSurveyAnswer.CreateFreeText(
            responseId,
            Guid.NewGuid(),
            questionId,
            "  Useful suggestion  ");

        Assert.Equal(QuestionType.FreeText, operatorAnswer.QuestionType);
        Assert.Equal("Excellent service", operatorAnswer.TextAnswer);
        Assert.Equal(QuestionType.FreeText, anonymousAnswer.QuestionType);
        Assert.Equal("Useful suggestion", anonymousAnswer.TextAnswer);
    }

    [Fact]
    public void OperatorFreeTextShape_AcceptsTextOnlyAndRejectsIncompatibleFields()
    {
        var valid = new SubmitOperatorTemplateAnswerCommandItem
        {
            QuestionId = Guid.NewGuid(),
            TextAnswer = "Normal feedback"
        };
        var invalid = valid with { StarRatingValue = 5 };

        Assert.Null(ValidateOperatorShape(valid));
        var error = Assert.IsType<Error>(ValidateOperatorShape(invalid));
        Assert.Equal("SurveyResponses.Submit.FreeTextInvalidShape", error.Code);
    }

    [Fact]
    public void AnonymousFreeTextShape_AcceptsTextOnlyAndRejectsIncompatibleFields()
    {
        var valid = new SubmitAnonymousTemplateAnswerCommandItem
        {
            AnonymousTemplateQuestionId = Guid.NewGuid(),
            TextAnswer = "Normal feedback"
        };
        var invalid = valid with { SmileValue = 4 };

        Assert.Null(ValidateAnonymousFreeText(valid));
        var error = Assert.IsType<Error>(ValidateAnonymousFreeText(invalid));
        Assert.Equal("AnonTemplates.Submit.FreeTextAnswerInvalid", error.Code);
    }

    [Fact]
    public void CustomInputValues_SnapshotLabelsForOperatorAndAnonymousResponses()
    {
        var operatorValue = SurveyResponseCustomInputValue.CreateStringValue(
            Guid.NewGuid(), Guid.NewGuid(), "Customer Mobile", "رقم الموبايل", "01012345678");
        var anonymousValue = AnonymousSurveyResponseCustomInputValue.CreateIntegerValue(
            Guid.NewGuid(), Guid.NewGuid(), "Age", "العمر", 30);

        Assert.Equal("Customer Mobile", operatorValue.LabelEnSnapshot);
        Assert.Equal("رقم الموبايل", operatorValue.LabelArSnapshot);
        Assert.Equal("Age", anonymousValue.LabelEnSnapshot);
        Assert.Equal("العمر", anonymousValue.LabelArSnapshot);
    }

    [Fact]
    public void CustomInputValues_KeepFullLabelsWithoutOverflowingLegacyNameSnapshot()
    {
        var longLabel = new string('L', 200);
        var operatorValue = SurveyResponseCustomInputValue.CreateStringValue(
            Guid.NewGuid(), Guid.NewGuid(), longLabel, null, "value");
        var anonymousValue = AnonymousSurveyResponseCustomInputValue.CreateStringValue(
            Guid.NewGuid(), Guid.NewGuid(), longLabel, null, "value");

        Assert.Equal(longLabel, operatorValue.LabelEnSnapshot);
        Assert.Equal(longLabel, anonymousValue.LabelEnSnapshot);
        Assert.Equal(100, operatorValue.NameSnapshot.Length);
        Assert.Equal(100, anonymousValue.NameSnapshot.Length);
    }

    [Fact]
    public void CustomInputValidation_RequiresEitherEnglishOrArabicLabel()
    {
        var validator = new CreateTemplateCommandValidator();

        Assert.False(validator.Validate(Command(null, "   ")).IsValid);
        Assert.True(validator.Validate(Command("Mobile", null)).IsValid);
        Assert.True(validator.Validate(Command(null, "رقم الموبايل")).IsValid);
    }

    private static CreateTemplateCommand Command(string? labelEn, string? labelAr)
        => new()
        {
            NameEn = "Customer Survey",
            ActiveFrom = DateTime.UtcNow,
            CustomInputs = new[]
            {
                new CreateTemplateCustomInputCommandItem
                {
                    LabelEn = labelEn,
                    LabelAr = labelAr,
                    Type = TemplateCustomInputType.String,
                    Order = 1
                }
            }
        };

    private static object? ValidateOperatorShape(SubmitOperatorTemplateAnswerCommandItem answer)
    {
        var method = typeof(SubmitOperatorTemplateResponseCommandHandler).GetMethod(
            "ValidateAnswerShape",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return method!.Invoke(null, new object[]
        {
            answer,
            QuestionType.FreeText,
            new Dictionary<Guid, HashSet<Guid>>()
        });
    }

    private static object? ValidateAnonymousFreeText(SubmitAnonymousTemplateAnswerCommandItem answer)
    {
        var method = typeof(SubmitAnonymousTemplateResponseCommandHandler).GetMethod(
            "ValidateFreeTextAnswer",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var result = method!.Invoke(null, new object[] { answer });
        Assert.NotNull(result);
        return result!.GetType().GetProperty("Error")!.GetValue(result);
    }
}
