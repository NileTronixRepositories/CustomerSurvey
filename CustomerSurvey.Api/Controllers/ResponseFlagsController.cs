using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Application.Features.ResponseFlags;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers;

[ApiController]
[Route("api/response-flags")]
[Authorize]
public sealed class ResponseFlagsController(ISender sender) : ControllerBase
{
    [HttpGet("responses")]
    [Permission(FlagAccess.ResponseFlagsView)]
    public async Task<IActionResult> Responses([FromQuery] GetFlaggedResponsesQuery query, CancellationToken ct)
        => (await sender.Send(query, ct)).ToIActionResult();

    [HttpGet("responses/{responseId:guid}")]
    [Permission(FlagAccess.ResponseFlagsView)]
    public async Task<IActionResult> ResponseDetails(Guid responseId, CancellationToken ct)
        => (await sender.Send(new GetFlaggedResponseQuery(responseId), ct)).ToIActionResult();

    [HttpGet("{flagId:guid}")]
    [Permission(FlagAccess.ResponseFlagsView)]
    public async Task<IActionResult> FlagDetails(Guid flagId, CancellationToken ct)
        => (await sender.Send(new GetResponseFlagQuery(flagId), ct)).ToIActionResult();

    [HttpPost("responses/{responseId:guid}")]
    [Permission(FlagAccess.ResponseFlagsCreate)]
    public async Task<IActionResult> Create(Guid responseId, [FromBody] CreateResponseFlagRequest request, CancellationToken ct)
        => (await sender.Send(new CreateResponseFlagCommand(responseId, request.Type, request.Brief, request.Answer), ct)).ToIActionResult();

    [HttpPut("{flagId:guid}/brief")]
    [Permission(FlagAccess.ResponseFlagsEditBrief)]
    public async Task<IActionResult> UpdateBrief(Guid flagId, [FromBody] UpdateFlagBriefRequest request, CancellationToken ct)
        => (await sender.Send(new UpdateResponseFlagBriefCommand(flagId, request.Brief), ct)).ToIActionResult();

    [HttpPut("{flagId:guid}/answer")]
    [Permission(FlagAccess.ResponseFlagsAnswer)]
    public async Task<IActionResult> WriteAnswer(Guid flagId, [FromBody] WriteFlagAnswerRequest request, CancellationToken ct)
        => (await sender.Send(new WriteResponseFlagAnswerCommand(flagId, request.Answer), ct)).ToIActionResult();
}

public sealed record CreateResponseFlagRequest(SurveyResponseFlagType Type, string Brief, string? Answer);
public sealed record UpdateFlagBriefRequest(string Brief);
public sealed record WriteFlagAnswerRequest(string Answer);
