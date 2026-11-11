using System.Reflection;
using BuildingBlock.Domain.Results;
using BuildingBlock.Application.Behaviors;
using CustomerSurvey.Api.Contracts.Operators;
using CustomerSurvey.Api.Controllers;
using CustomerSurvey.Application.Features.ResponseFlags;
using CustomerSurvey.Application.Features.FlagOfficers;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CustomerSurvey.Tests.ResponseFlags;

public sealed class ResponseFlagValidationTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  Feedback  ", "Feedback")]
    public void OperatorBriefIsOptional(string? brief, string? expected)
    {
        var request = SubmitRequest(new SubmitSurveyFlagCommandItem(SurveyResponseFlagType.Suggestion, brief));
        Assert.True(new SubmitOperatorTemplateResponseCommandValidator().Validate(request).IsValid);

        var userId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var response = SurveyResponse.Create(operatorId, request.TemplateId, userId, 0, 0, 0);
        response.AddSubmittedFlag(SurveyResponseFlagType.Suggestion, brief);
        response.AddSubmittedFlag(SurveyResponseFlagType.Suggestion, null);
        Assert.Equal(2, response.Flags.Count);
        var flag = response.Flags.First();
        Assert.Equal(expected, flag.Brief);
        Assert.Null(flag.Answer);
        Assert.Equal(userId, flag.CreatedByApplicationUserId);
        Assert.Equal(operatorId, response.OperatorId);
    }

    [Fact]
    public void OperatorMayOmitFlagsAndMustUseDefinedTypes()
    {
        var validator = new SubmitOperatorTemplateResponseCommandValidator();
        Assert.True(validator.Validate(SubmitRequest()).IsValid);
        Assert.False(validator.Validate(SubmitRequest(new SubmitSurveyFlagCommandItem((SurveyResponseFlagType)0, null))).IsValid);
        Assert.False(validator.Validate(SubmitRequest(new SubmitSurveyFlagCommandItem(
            SurveyResponseFlagType.Comment, new string('x', SurveyResponseFlag.MaxBriefLength + 1)))).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OfficerMustProvideNonEmptyTextWhenCreatingOrEditingBrief(string? brief)
    {
        var id = Guid.NewGuid();
        Assert.False(new CreateResponseFlagValidator().Validate(
            new CreateResponseFlagCommand(id, SurveyResponseFlagType.Complaint, brief!, null)).IsValid);
        Assert.False(new UpdateResponseFlagBriefValidator().Validate(new UpdateResponseFlagBriefCommand(id, brief!)).IsValid);
        Assert.False(new WriteResponseFlagAnswerValidator().Validate(new WriteResponseFlagAnswerCommand(id, brief!)).IsValid);
        Assert.True(new CreateResponseFlagValidator().Validate(
            new CreateResponseFlagCommand(id, SurveyResponseFlagType.Complaint, "Valid brief", null)).IsValid);
    }

    [Theory]
    [InlineData("Flags[0].Answer")]
    [InlineData("flags[0][answer]")]
    [InlineData("request.Flags[0].Answer")]
    public async Task OperatorFormRejectsAnswerBeforeDispatch(string field)
    {
        var sender = DispatchProxy.Create<ISender, OperatorSender>();
        var http = new DefaultHttpContext();
        http.Request.ContentType = "multipart/form-data; boundary=test";
        http.Request.Form = new FormCollection(new Dictionary<string, StringValues> { [field] = "unauthorized" });
        var controller = new OperatorsController(sender) { ControllerContext = new ControllerContext { HttpContext = http } };
        var result = await controller.SubmitTemplateResponse(Guid.NewGuid(), new SubmitOperatorTemplateResponseRequest(), default);
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        http.RequestServices = services;
        await using var responseBody = new MemoryStream();
        http.Response.Body = responseBody;
        await result.ExecuteResultAsync(controller.ControllerContext);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, http.Response.StatusCode);
        Assert.Null(((OperatorSender)(object)sender).Request);
    }

    [Fact]
    public async Task OperatorFormForwardsTypeOnlyFlag()
    {
        var sender = DispatchProxy.Create<ISender, OperatorSender>();
        var http = new DefaultHttpContext();
        http.Request.ContentType = "multipart/form-data; boundary=test";
        http.Request.Form = new FormCollection(new Dictionary<string, StringValues> { ["Flags[0].Type"] = "1" });
        var controller = new OperatorsController(sender) { ControllerContext = new ControllerContext { HttpContext = http } };
        var request = new SubmitOperatorTemplateResponseRequest
        {
            Flags = [new SubmitSurveyFlagRequest { Type = SurveyResponseFlagType.Complaint }]
        };
        await controller.SubmitTemplateResponse(Guid.NewGuid(), request, default);
        Assert.Null(Assert.Single(Assert.IsType<SubmitOperatorTemplateResponseCommand>(
            ((OperatorSender)(object)sender).Request).Flags).Brief);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(30, true)]
    [InlineData(31, false)]
    public async Task OfficerCreateAndUpdateValidatePhoneBeforePersistence(int? length, bool accepted)
    {
        var profile = new FlagOfficerProfile("Flag Officer", null, "officer@example.test",
            length.HasValue ? new string('1', length.Value) : null);
        var create = new CreateFlagOfficerCommand(Guid.NewGuid(), "officer", "TestPassword123", profile);
        var update = new UpdateFlagOfficerCommand(Guid.NewGuid(), profile);
        var handlerCalls = 0;
        Task<Result<FlagOfficerChange>> Persist(CancellationToken ct)
        {
            handlerCalls++;
            return Task.FromResult(Result<FlagOfficerChange>.Ok(new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true)));
        }

        var createResult = await new ValidationBehavior<CreateFlagOfficerCommand, Result<FlagOfficerChange>>(
            [new CreateFlagOfficerValidator()]).Handle(create, Persist, default);
        var updateResult = await new ValidationBehavior<UpdateFlagOfficerCommand, Result<FlagOfficerChange>>(
            [new UpdateFlagOfficerValidator()]).Handle(update, Persist, default);

        Assert.Equal(accepted, createResult.IsSuccess);
        Assert.Equal(accepted, updateResult.IsSuccess);
        Assert.Equal(accepted ? 2 : 0, handlerCalls);
        if (!accepted)
        {
            Assert.Equal(ErrorType.Validation, Assert.Single(createResult.Errors).Type);
            Assert.Equal(ErrorType.Validation, Assert.Single(updateResult.Errors).Type);
        }
    }

    private static SubmitOperatorTemplateResponseCommand SubmitRequest(params SubmitSurveyFlagCommandItem[] flags)
        => new()
        {
            TemplateId = Guid.NewGuid(),
            Answers = [new SubmitOperatorTemplateAnswerCommandItem { QuestionId = Guid.NewGuid() }],
            Flags = flags
        };

    public class OperatorSender : DispatchProxy
    {
        public object? Request { get; private set; }
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            Request = args![0];
            return Task.FromResult(Result<SubmitOperatorTemplateResponseResponse>.Ok(new()));
        }
    }
}
