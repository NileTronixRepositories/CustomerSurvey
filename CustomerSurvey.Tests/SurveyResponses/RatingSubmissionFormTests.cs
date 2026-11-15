using System.Net;
using BuildingBlock.Application.Behaviors;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Api.Controllers;
using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CustomerSurvey.Tests.SurveyResponses;

public sealed class RatingSubmissionFormTests
{
    [Fact]
    public async Task Multipart_submissions_bind_duration_and_rating_and_preserve_validation()
    {
        var operatorSubmission = new CaptureSubmission<SubmitOperatorTemplateResponseCommand, SubmitOperatorTemplateResponseResponse>();
        var anonymousSubmission = new CaptureSubmission<SubmitAnonymousTemplateResponseCommand, SubmitAnonymousTemplateResponseResult>();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllers().AddApplicationPart(typeof(OperatorsController).Assembly);
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build())
            .AddPolicy("PERMISSION_OperatorTemplates.SubmitResponse", policy => policy.RequireAssertion(_ => true));
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<RatingSubmissionFormTests>();
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        builder.Services.AddSingleton<IValidator<SubmitOperatorTemplateResponseCommand>, SubmitOperatorTemplateResponseCommandValidator>();
        builder.Services.AddSingleton<IRequestHandler<SubmitOperatorTemplateResponseCommand, Result<SubmitOperatorTemplateResponseResponse>>>(operatorSubmission);
        builder.Services.AddSingleton<IRequestHandler<SubmitAnonymousTemplateResponseCommand, Result<SubmitAnonymousTemplateResponseResult>>>(anonymousSubmission);
        await using var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();

        using var client = new HttpClient { BaseAddress = new Uri(Assert.Single(app.Urls)) };
        var templateId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var operatorRoute = $"/api/operators/my-templates/{templateId}/responses";
        using var normalForm = RatingForm("QuestionId", questionId, 9);
        normalForm.Add(new StringContent("225"), "AnswerDurationSeconds");
        using var normal = await client.PostAsync(operatorRoute, normalForm);
        Assert.Equal(HttpStatusCode.OK, normal.StatusCode);
        var command = Assert.IsType<SubmitOperatorTemplateResponseCommand>(operatorSubmission.Request);
        Assert.Equal(templateId, command.TemplateId);
        Assert.Equal(225, command.AnswerDurationSeconds);
        var answer = Assert.Single(command.Answers);
        Assert.Equal(questionId, answer.QuestionId);
        Assert.Equal(9, answer.Rating1To10Value);

        using var negativeForm = RatingForm("QuestionId", questionId, 9);
        negativeForm.Add(new StringContent("-1"), "AnswerDurationSeconds");
        using var negative = await client.PostAsync(operatorRoute, negativeForm);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, negative.StatusCode);
        Assert.Same(command, operatorSubmission.Request);

        using var flagForm = RatingForm("QuestionId", questionId, 9);
        flagForm.Add(new StringContent("Disallowed"), "Flags[0].Answer");
        using var flagAnswer = await client.PostAsync(operatorRoute, flagForm);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, flagAnswer.StatusCode);
        Assert.Contains("ResponseFlags.AnswerNotAllowedOnSubmit", await flagAnswer.Content.ReadAsStringAsync());
        Assert.Same(command, operatorSubmission.Request);

        var anonymousTemplateId = Guid.NewGuid();
        var anonymousTemplateQuestionId = Guid.NewGuid();
        using var anonymousForm = RatingForm("AnonymousTemplateQuestionId", anonymousTemplateQuestionId, 10);
        using var anonymous = await client.PostAsync($"/api/anon-templates/{anonymousTemplateId}/responses", anonymousForm);
        Assert.Equal(HttpStatusCode.OK, anonymous.StatusCode);
        var publicCommand = Assert.IsType<SubmitAnonymousTemplateResponseCommand>(anonymousSubmission.Request);
        Assert.Equal(anonymousTemplateId, publicCommand.AnonymousTemplateId);
        var publicAnswer = Assert.Single(publicCommand.Answers);
        Assert.Equal(anonymousTemplateQuestionId, publicAnswer.AnonymousTemplateQuestionId);
        Assert.Equal(10, publicAnswer.Rating1To10Value);
    }

    private static MultipartFormDataContent RatingForm(string questionIdField, Guid questionId, int rating)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(questionId.ToString()), $"Answers[0].{questionIdField}");
        form.Add(new StringContent(rating.ToString()), "Answers[0].Rating1To10Value");
        return form;
    }

    private sealed class CaptureSubmission<TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
        where TRequest : IRequest<Result<TResponse>>
        where TResponse : new()
    {
        public TRequest? Request { get; private set; }

        public Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(Result<TResponse>.Ok(new TResponse()));
        }
    }
}
