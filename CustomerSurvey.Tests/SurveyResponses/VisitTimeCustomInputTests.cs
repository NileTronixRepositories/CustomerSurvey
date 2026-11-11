using System.Reflection;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Application.Features.Templates.Command.CreateTemplate;
using CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Xunit;

namespace CustomerSurvey.Tests.SurveyResponses;

public sealed class VisitTimeCustomInputTests
{
    [Fact]
    public void NormalBranchAndGlobalTemplatesCreateOneOptionalVisitTime()
    {
        var owner = Guid.NewGuid();
        var branch = Guid.NewGuid();
        var template = Template.Create(branch, "Authorized", null, null, DateTime.UtcNow, null, owner);
        var anonymous = AnonymousTemplate.CreateBranchTemplate(branch, "Anonymous", null, null, DateTime.UtcNow, null, owner);
        var global = AnonymousTemplate.CreateGlobalTemplate("Global", null, null, DateTime.UtcNow, null, owner);

        var normalVisitTime = Assert.Single(template.CustomInputs);
        Assert.Equal((TemplateCustomInputSystemField.VisitTime, "Visit Time", "تاريخ الزيارة", TemplateCustomInputType.Date, false, true, owner),
            (normalVisitTime.SystemFieldKey, normalVisitTime.LabelEn, normalVisitTime.LabelAr, normalVisitTime.Type,
                normalVisitTime.IsRequired, normalVisitTime.IsActive, normalVisitTime.CreatedByApplicationUserId));

        foreach (var item in new[] { anonymous, global })
        {
            var visitTime = Assert.Single(item.CustomInputs);
            Assert.Equal((TemplateCustomInputSystemField.VisitTime, "Visit Time", "تاريخ الزيارة", TemplateCustomInputType.Date, false, true, owner),
                (visitTime.SystemFieldKey, visitTime.LabelEn, visitTime.LabelAr, visitTime.Type,
                    visitTime.IsRequired, visitTime.IsActive, visitTime.CreatedByApplicationUserId));
        }

        var ordinary = TemplateCustomInput.Create(template.Id, "Other date", null, TemplateCustomInputType.Date,
            true, 1, 2, 3, 4, "prefix", 1, owner);
        template.AddCustomInput(ordinary);
        Assert.Null(ordinary.SystemFieldKey);
        Assert.Equal((null, null, null, null, null),
            (ordinary.MinLength, ordinary.MaxLength, ordinary.MinValue, ordinary.MaxValue, ordinary.StartWith));
        Assert.Equal(2, normalVisitTime.Order);

        var dateCommand = new CreateTemplateCommand
        {
            NameEn = "Authorized",
            ActiveFrom = DateTime.UtcNow,
            CustomInputs = [new() { LabelEn = "Other date", Type = TemplateCustomInputType.Date, Order = 1 }]
        };
        Assert.True(new CreateTemplateCommandValidator().Validate(dateCommand).IsValid);
    }

    [Fact]
    public void TemplateUpdatesRetainAndProtectTheSystemField()
    {
        var owner = Guid.NewGuid();
        var branch = Guid.NewGuid();
        var template = Template.Create(branch, "Authorized", null, null, DateTime.UtcNow, null, owner);
        var anonymous = AnonymousTemplate.CreateBranchTemplate(branch, "Anonymous", null, null, DateTime.UtcNow, null, owner);
        var visitTime = Assert.Single(template.CustomInputs);
        var anonymousVisitTime = Assert.Single(anonymous.CustomInputs);

        var normalRequest = new UpdateTemplateCustomInputCommandItem
        {
            CustomInputId = visitTime.Id, Type = visitTime.Type, Order = visitTime.Order,
            LabelEn = visitTime.LabelEn, LabelAr = visitTime.LabelAr, IsRequired = visitTime.IsRequired
        };
        var anonymousRequest = new UpdateAnonymousTemplateCustomInputCommandItem
        {
            CustomInputId = anonymousVisitTime.Id, Type = anonymousVisitTime.Type, Order = anonymousVisitTime.Order,
            LabelEn = anonymousVisitTime.LabelEn, LabelAr = anonymousVisitTime.LabelAr
        };

        Error? UpdateNormal(params UpdateTemplateCustomInputCommandItem[] requests)
            => ErrorOf(Invoke(typeof(UpdateTemplateCommandHandler), "ApplyCustomInputsUpdate",
                template.Id, template.CustomInputs, requests, owner));
        Error? UpdateAnonymous(params UpdateAnonymousTemplateCustomInputCommandItem[] requests)
            => ErrorOf(Invoke(typeof(UpdateAnonymousTemplateCommandHandler), "ApplyCustomInputsUpdate",
                anonymous.Id, anonymous.CustomInputs, requests, owner));

        Assert.Null(UpdateNormal(normalRequest));
        Assert.Null(UpdateAnonymous(anonymousRequest));
        Assert.Null(UpdateNormal(new UpdateTemplateCustomInputCommandItem { LabelEn = "Name", Type = TemplateCustomInputType.String, Order = 1 }));
        Assert.Null(UpdateAnonymous(new UpdateAnonymousTemplateCustomInputCommandItem { LabelEn = "Name", Type = TemplateCustomInputType.String, Order = 1 }));
        Assert.Equal(2, visitTime.Order);
        Assert.Equal(2, anonymousVisitTime.Order);
        Assert.True(visitTime.IsActive);
        Assert.True(anonymousVisitTime.IsActive);

        Assert.Equal(ErrorType.Validation, UpdateNormal(normalRequest with { LabelEn = "Changed" })!.Type);
        Assert.Equal(ErrorType.Validation, UpdateNormal(normalRequest with { Type = TemplateCustomInputType.String })!.Type);
        Assert.Equal(ErrorType.Validation, UpdateNormal(normalRequest, normalRequest)!.Type);
        Assert.Equal(ErrorType.Validation, UpdateAnonymous(anonymousRequest with { IsRequired = true })!.Type);
        Assert.Equal(ErrorType.Validation, UpdateAnonymous(anonymousRequest with { LabelAr = null })!.Type);
        Assert.Equal(ErrorType.Validation, UpdateAnonymous(anonymousRequest, anonymousRequest)!.Type);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("2026-11-08", null)]
    [InlineData("0001-01-01", null)]
    [InlineData("9999-12-31", null)]
    [InlineData("2026-11-8", "CustomInputDateInvalid")]
    [InlineData("2026-02-29", "CustomInputDateInvalid")]
    [InlineData("2026-11-08T00:00:00Z", "CustomInputDateInvalid")]
    public void BothSubmissionsValidateExactDatesAndSaveOnlyDateValue(string? submitted, string? expectedError)
    {
        var inputId = Guid.NewGuid();
        var operatorInputs = new[] { new TemplateCustomInputForSubmitResponseDto
        {
            CustomInputId = inputId, Type = TemplateCustomInputType.Date, LabelEn = "Visit Time", LabelAr = "تاريخ الزيارة"
        }};
        var anonymousInputs = new[] { new AnonymousTemplateCustomInputForSubmitDto
        {
            CustomInputId = inputId, Type = TemplateCustomInputType.Date, LabelEn = "Visit Time", LabelAr = "تاريخ الزيارة"
        }};
        SubmitOperatorTemplateCustomInputCommandItem[] operatorValues = submitted is null ? [] : [new() { CustomInputId = inputId, Value = submitted }];
        SubmitAnonymousTemplateCustomInputValueCommandItem[] anonymousValues = submitted is null ? [] : [new() { CustomInputId = inputId, DateValue = submitted }];
        var operatorHandler = typeof(SubmitOperatorTemplateResponseCommandHandler);
        var anonymousHandler = typeof(SubmitAnonymousTemplateResponseCommandHandler);
        var operatorError = (Error?)Invoke(operatorHandler, "ValidateCustomInputs", operatorInputs, operatorValues);
        var anonymousError = ErrorOf(Invoke(anonymousHandler, "ValidateCustomInputValues", anonymousInputs, anonymousValues));

        if (expectedError is not null)
        {
            Assert.EndsWith(expectedError, operatorError!.Code);
            Assert.EndsWith(expectedError, anonymousError!.Code);
            Assert.Equal(ErrorType.Validation, operatorError.Type);
            Assert.Equal(ErrorType.Validation, anonymousError.Type);
            return;
        }

        Assert.Null(operatorError);
        Assert.Null(anonymousError);
        var operatorRows = Assert.IsAssignableFrom<IEnumerable<SurveyResponseCustomInputValue>>(
            Invoke(operatorHandler, "BuildCustomInputValues", Guid.NewGuid(), operatorInputs, operatorValues));
        var anonymousResponse = AnonymousSurveyResponse.Create(Guid.NewGuid(), 0, 0, 0);
        Invoke(anonymousHandler, "AddCustomInputValuesToResponse", anonymousResponse, anonymousInputs, anonymousValues);

        if (string.IsNullOrEmpty(submitted))
        {
            Assert.Empty(operatorRows);
            Assert.Empty(anonymousResponse.CustomInputValues);
            return;
        }

        var date = DateOnly.ParseExact(submitted, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        var normal = Assert.Single(operatorRows);
        var anonymous = Assert.Single(anonymousResponse.CustomInputValues);
        Assert.Equal((inputId, TemplateCustomInputType.Date, date, null, null, "تاريخ الزيارة"),
            (normal.TemplateCustomInputId, normal.TypeSnapshot, normal.DateValue, normal.StringValue, normal.IntegerValue, normal.LabelArSnapshot));
        Assert.Equal((inputId, TemplateCustomInputType.Date, date, null, null, "تاريخ الزيارة"),
            (anonymous.AnonymousTemplateCustomInputId, anonymous.TypeSnapshot, anonymous.DateValue, anonymous.StringValue, anonymous.IntegerValue, anonymous.LabelArSnapshot));
    }

    [Fact]
    public void BothSubmissionsRejectDuplicateAndForeignIdsAndAnonymousRejectsWrongShapes()
    {
        var inputId = Guid.NewGuid();
        var operatorInputs = new[] { new TemplateCustomInputForSubmitResponseDto { CustomInputId = inputId, Type = TemplateCustomInputType.Date } };
        var anonymousInputs = new[] { new AnonymousTemplateCustomInputForSubmitDto { CustomInputId = inputId, Type = TemplateCustomInputType.Date } };
        var normal = new SubmitOperatorTemplateCustomInputCommandItem { CustomInputId = inputId, Value = "2026-11-08" };
        var anonymous = new SubmitAnonymousTemplateCustomInputValueCommandItem { CustomInputId = inputId, DateValue = "2026-11-08" };

        Error OperatorError(params SubmitOperatorTemplateCustomInputCommandItem[] values)
            => Assert.IsType<Error>(Invoke(typeof(SubmitOperatorTemplateResponseCommandHandler), "ValidateCustomInputs", operatorInputs, values));
        Error AnonymousError(params SubmitAnonymousTemplateCustomInputValueCommandItem[] values)
            => Assert.IsType<Error>(ErrorOf(Invoke(typeof(SubmitAnonymousTemplateResponseCommandHandler), "ValidateCustomInputValues", anonymousInputs, values)));

        Assert.EndsWith("CustomInputDuplicated", OperatorError(normal, normal).Code);
        Assert.EndsWith("CustomInputNotFound", OperatorError(new SubmitOperatorTemplateCustomInputCommandItem { CustomInputId = Guid.NewGuid(), Value = normal.Value }).Code);
        Assert.EndsWith("CustomInputDuplicated", AnonymousError(anonymous, anonymous).Code);
        Assert.EndsWith("CustomInputNotFound", AnonymousError(anonymous with { CustomInputId = Guid.NewGuid() }).Code);
        Assert.EndsWith("CustomInputDateShapeInvalid", AnonymousError(anonymous with { IntegerValue = 8 }).Code);
        Assert.EndsWith("CustomInputDateShapeInvalid", AnonymousError(anonymous with { StringValue = "2026-11-08" }).Code);
    }

    private static object? Invoke(Type handler, string name, params object[] arguments)
        => handler.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, arguments);

    private static Error? ErrorOf(object? result)
        => (Error?)result!.GetType().GetProperty("Error")!.GetValue(result);
}
