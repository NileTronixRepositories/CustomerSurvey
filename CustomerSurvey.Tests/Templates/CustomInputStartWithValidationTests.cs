using System.Reflection;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Application.Features.Templates.Command.CreateTemplate;
using CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using Xunit;

namespace CustomerSurvey.Tests.Templates;

public sealed class CustomInputStartWithValidationTests
{
    [Theory]
    [MemberData(nameof(ValidCreateTemplateCommands))]
    public void CreateTemplateValidators_AllowStringCustomInputWithStartWith(object command, object validator)
    {
        var result = Validate(command, validator);

        Assert.True(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(InvalidNonStringCreateTemplateCommands))]
    public void CreateTemplateValidators_RejectStartWithForNonStringCustomInput(
        object command,
        object validator,
        string expectedMessage)
    {
        var result = Validate(command, validator);

        Assert.Contains(result.Errors, error => error.ErrorMessage == expectedMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidWhitespaceCreateTemplateCommands))]
    public void CreateTemplateValidators_RejectWhitespaceStartWith(
        object command,
        object validator,
        string expectedMessage)
    {
        var result = Validate(command, validator);

        Assert.Contains(result.Errors, error => error.ErrorMessage == expectedMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidLongCreateTemplateCommands))]
    public void CreateTemplateValidators_RejectStartWithLongerThan100Characters(
        object command,
        object validator,
        string expectedMessage)
    {
        var result = Validate(command, validator);

        Assert.Contains(result.Errors, error => error.ErrorMessage == expectedMessage);
    }

    [Fact]
    public void TemplateCustomInput_Create_TrimsStringStartWith()
    {
        var customInput = TemplateCustomInput.Create(
            templateId: Guid.NewGuid(),
            name: "CustomerPhone",
            labelEn: null,
            labelAr: null,
            type: TemplateCustomInputType.String,
            isRequired: true,
            minLength: null,
            maxLength: null,
            minValue: null,
            maxValue: null,
            startWith: " 011 ",
            order: 1,
            createdByApplicationUserId: Guid.NewGuid());

        Assert.Equal("011", customInput.StartWith);
    }

    [Fact]
    public void TemplateCustomInput_Create_ClearsStartWithForInteger()
    {
        var customInput = TemplateCustomInput.Create(
            templateId: Guid.NewGuid(),
            name: "CustomerAge",
            labelEn: null,
            labelAr: null,
            type: TemplateCustomInputType.Integer,
            isRequired: true,
            minLength: null,
            maxLength: null,
            minValue: 1,
            maxValue: 100,
            startWith: "011",
            order: 1,
            createdByApplicationUserId: Guid.NewGuid());

        Assert.Null(customInput.StartWith);
    }

    [Fact]
    public void SubmitOperatorTemplateResponseValidation_RejectsStringValueThatDoesNotStartWithConfiguredPrefix()
    {
        var customInput = new TemplateCustomInputForSubmitResponseDto
        {
            CustomInputId = Guid.NewGuid(),
            TemplateId = Guid.NewGuid(),
            Name = "CustomerPhone",
            Type = TemplateCustomInputType.String,
            IsRequired = true,
            StartWith = "011",
            Order = 1
        };

        var method = typeof(SubmitOperatorTemplateResponseCommandHandler).GetMethod(
            "ValidateStringCustomInput",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var error = Assert.IsType<Error>(
            method!.Invoke(null, new object?[] { customInput, "01023456789" }));

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(
            ErrorMessage.SubmitTemplateResponse_CustomInput_StartWith_Invalid,
            error.Message);
    }

    public static TheoryData<object, object> ValidCreateTemplateCommands()
        => new()
        {
            { CreateAuthorizedCreateCommand(startWith: "011"), new CreateTemplateCommandValidator() },
            { CreateAnonymousCreateCommand(startWith: "011"), new CreateAnonymousTemplateCommandValidator() },
            { CreateAuthorizedUpdateCommand(startWith: "011"), new UpdateTemplateCommandValidator() },
            { CreateAnonymousUpdateCommand(startWith: "011"), new UpdateAnonymousTemplateCommandValidator() }
        };

    public static TheoryData<object, object, string> InvalidNonStringCreateTemplateCommands()
        => new()
        {
            {
                CreateAuthorizedCreateCommand(type: TemplateCustomInputType.Integer, startWith: "011"),
                new CreateTemplateCommandValidator(),
                ErrorMessage.CreateTemplate_CustomInput_StartWith_NotAllowed
            },
            {
                CreateAnonymousCreateCommand(type: TemplateCustomInputType.Integer, startWith: "011"),
                new CreateAnonymousTemplateCommandValidator(),
                ErrorMessage.CreateAnonymousTemplate_CustomInput_StartWith_NotAllowed
            },
            {
                CreateAuthorizedUpdateCommand(type: TemplateCustomInputType.Integer, startWith: "011"),
                new UpdateTemplateCommandValidator(),
                ErrorMessage.UpdateTemplate_CustomInput_StartWith_NotAllowed
            },
            {
                CreateAnonymousUpdateCommand(type: TemplateCustomInputType.Integer, startWith: "011"),
                new UpdateAnonymousTemplateCommandValidator(),
                ErrorMessage.UpdateAnonymousTemplate_CustomInput_StartWith_NotAllowed
            }
        };

    public static TheoryData<object, object, string> InvalidWhitespaceCreateTemplateCommands()
        => new()
        {
            {
                CreateAuthorizedCreateCommand(startWith: "   "),
                new CreateTemplateCommandValidator(),
                ErrorMessage.CreateTemplate_CustomInput_StartWith_Empty
            },
            {
                CreateAnonymousCreateCommand(startWith: "   "),
                new CreateAnonymousTemplateCommandValidator(),
                ErrorMessage.CreateAnonymousTemplate_CustomInput_StartWith_Empty
            },
            {
                CreateAuthorizedUpdateCommand(startWith: "   "),
                new UpdateTemplateCommandValidator(),
                ErrorMessage.UpdateTemplate_CustomInput_StartWith_Empty
            },
            {
                CreateAnonymousUpdateCommand(startWith: "   "),
                new UpdateAnonymousTemplateCommandValidator(),
                ErrorMessage.UpdateAnonymousTemplate_CustomInput_StartWith_Empty
            }
        };

    public static TheoryData<object, object, string> InvalidLongCreateTemplateCommands()
        => new()
        {
            {
                CreateAuthorizedCreateCommand(startWith: new string('1', 101)),
                new CreateTemplateCommandValidator(),
                ErrorMessage.CreateTemplate_CustomInput_StartWith_MaxLength
            },
            {
                CreateAnonymousCreateCommand(startWith: new string('1', 101)),
                new CreateAnonymousTemplateCommandValidator(),
                ErrorMessage.CreateAnonymousTemplate_CustomInput_StartWith_MaxLength
            },
            {
                CreateAuthorizedUpdateCommand(startWith: new string('1', 101)),
                new UpdateTemplateCommandValidator(),
                ErrorMessage.UpdateTemplate_CustomInput_StartWith_MaxLength
            },
            {
                CreateAnonymousUpdateCommand(startWith: new string('1', 101)),
                new UpdateAnonymousTemplateCommandValidator(),
                ErrorMessage.UpdateAnonymousTemplate_CustomInput_StartWith_MaxLength
            }
        };

    private static FluentValidation.Results.ValidationResult Validate(object command, object validator)
    {
        var method = validator.GetType().GetMethod(
            "Validate",
            new[] { command.GetType() });

        Assert.NotNull(method);

        return Assert.IsType<FluentValidation.Results.ValidationResult>(
            method!.Invoke(validator, new[] { command }));
    }

    private static CreateTemplateCommand CreateAuthorizedCreateCommand(
        TemplateCustomInputType type = TemplateCustomInputType.String,
        string? startWith = null)
        => new()
        {
            NameEn = "Customer Visit",
            ActiveFrom = DateTime.UtcNow,
            CustomInputs = new[]
            {
                new CreateTemplateCustomInputCommandItem
                {
                    Name = "CustomerPhone",
                    Type = type,
                    IsRequired = true,
                    MinValue = type == TemplateCustomInputType.Integer ? 1 : null,
                    MaxValue = type == TemplateCustomInputType.Integer ? 100 : null,
                    StartWith = startWith,
                    Order = 1
                }
            }
        };

    private static CreateAnonymousTemplateCommand CreateAnonymousCreateCommand(
        TemplateCustomInputType type = TemplateCustomInputType.String,
        string? startWith = null)
        => new()
        {
            Scope = AnonymousTemplateScope.Branch,
            NameEn = "Anonymous Visit",
            ActiveFrom = DateTime.UtcNow,
            CustomInputs = new[]
            {
                new CreateAnonymousTemplateCustomInputCommandItem
                {
                    Name = "CustomerPhone",
                    Type = type,
                    IsRequired = true,
                    MinValue = type == TemplateCustomInputType.Integer ? 1 : null,
                    MaxValue = type == TemplateCustomInputType.Integer ? 100 : null,
                    StartWith = startWith,
                    Order = 1
                }
            }
        };

    private static UpdateTemplateCommand CreateAuthorizedUpdateCommand(
        TemplateCustomInputType type = TemplateCustomInputType.String,
        string? startWith = null)
        => new()
        {
            TemplateId = Guid.NewGuid(),
            NameEn = "Customer Visit",
            ActiveFrom = DateTime.UtcNow,
            CustomInputs = new[]
            {
                new UpdateTemplateCustomInputCommandItem
                {
                    CustomInputId = Guid.NewGuid(),
                    Name = "CustomerPhone",
                    Type = type,
                    IsRequired = true,
                    MinValue = type == TemplateCustomInputType.Integer ? 1 : null,
                    MaxValue = type == TemplateCustomInputType.Integer ? 100 : null,
                    StartWith = startWith,
                    Order = 1
                }
            }
        };

    private static UpdateAnonymousTemplateCommand CreateAnonymousUpdateCommand(
        TemplateCustomInputType type = TemplateCustomInputType.String,
        string? startWith = null)
        => new()
        {
            AnonymousTemplateId = Guid.NewGuid(),
            NameEn = "Anonymous Visit",
            ActiveFrom = DateTime.UtcNow,
            CustomInputs = new[]
            {
                new UpdateAnonymousTemplateCustomInputCommandItem
                {
                    CustomInputId = Guid.NewGuid(),
                    Name = "CustomerPhone",
                    Type = type,
                    IsRequired = true,
                    MinValue = type == TemplateCustomInputType.Integer ? 1 : null,
                    MaxValue = type == TemplateCustomInputType.Integer ? 100 : null,
                    StartWith = startWith,
                    Order = 1
                }
            }
        };
}
