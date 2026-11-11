using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Application.Features.FlagOfficers;
using CustomerSurvey.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers;

[ApiController]
[Route("api/flag-officers")]
[Authorize]
public sealed class FlagOfficersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Permission(FlagAccess.FlagOfficersCreate)]
    public async Task<IActionResult> Create([FromBody] CreateFlagOfficerRequest request, CancellationToken ct)
        => (await sender.Send(new CreateFlagOfficerCommand(request.DepartmentId, request.UserName, request.Password,
            new FlagOfficerProfile(request.NameEn, request.NameAr, request.Email, request.PhoneNumber)), ct)).ToIActionResult();

    [HttpGet]
    [Permission(FlagAccess.FlagOfficersViewAll)]
    public async Task<IActionResult> List([FromQuery] GetFlagOfficersQuery request, CancellationToken ct)
        => (await sender.Send(request, ct)).ToIActionResult();

    [HttpGet("{flagOfficerId:guid}")]
    [Permission(FlagAccess.FlagOfficersViewDetails)]
    public async Task<IActionResult> Details(Guid flagOfficerId, CancellationToken ct)
        => (await sender.Send(new GetFlagOfficerQuery(flagOfficerId), ct)).ToIActionResult();

    [HttpPut("{flagOfficerId:guid}")]
    [Permission(FlagAccess.FlagOfficersUpdate)]
    public async Task<IActionResult> Update(Guid flagOfficerId, [FromBody] FlagOfficerProfile request, CancellationToken ct)
        => (await sender.Send(new UpdateFlagOfficerCommand(flagOfficerId, request), ct)).ToIActionResult();

    [HttpPut("{flagOfficerId:guid}/deactivate")]
    [Permission(FlagAccess.FlagOfficersDeactivate)]
    public async Task<IActionResult> Deactivate(Guid flagOfficerId, CancellationToken ct)
        => (await sender.Send(new DeactivateFlagOfficerCommand(flagOfficerId), ct)).ToIActionResult();

    [HttpPut("{flagOfficerId:guid}/restore")]
    [Permission(FlagAccess.FlagOfficersRestore)]
    public async Task<IActionResult> Restore(Guid flagOfficerId, CancellationToken ct)
        => (await sender.Send(new RestoreFlagOfficerCommand(flagOfficerId), ct)).ToIActionResult();

    [HttpGet("roles-selection")]
    [Permission(FlagAccess.FlagOfficersAssignRoles)]
    public async Task<IActionResult> RoleSelection(CancellationToken ct)
        => (await sender.Send(new GetFlagOfficerRolesQuery(), ct)).ToIActionResult();

    [HttpPut("{flagOfficerId:guid}/roles")]
    [Permission(FlagAccess.FlagOfficersAssignRoles)]
    public async Task<IActionResult> AssignRoles(Guid flagOfficerId, [FromBody] AssignFlagOfficerRolesRequest request, CancellationToken ct)
        => (await sender.Send(new AssignFlagOfficerRolesCommand(flagOfficerId, request.RoleIds), ct)).ToIActionResult();
}

public sealed record CreateFlagOfficerRequest(
    Guid? DepartmentId, string UserName, string Password, string NameEn, string? NameAr, string Email, string? PhoneNumber);

public sealed record AssignFlagOfficerRolesRequest(IReadOnlyCollection<Guid> RoleIds);
