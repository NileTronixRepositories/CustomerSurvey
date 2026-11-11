using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.ResponseFlags;

internal sealed class CreateResponseFlagHandler(
    FlagOfficerAccess access,
    FlagPermissions permissions,
    IWriteReadRepository<SurveyResponse> responses,
    IWriteRepository<SurveyResponseFlag> flags,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateResponseFlagCommand, ResponseFlagChange>
{
    public async Task<Result<ResponseFlagChange>> Handle(CreateResponseFlagCommand request, CancellationToken ct)
    {
        var actor = await access.RequireAsync(FlagAccess.ResponseFlagsCreate, ct);
        if (actor.IsFailure)
            return Result<ResponseFlagChange>.Fail(actor.Errors);

        if (request.Answer is not null &&
            !await permissions.HasAsync(actor.Value, UserType.FlagOfficer, FlagAccess.ResponseFlagsAnswer, ct))
        {
            return Result<ResponseFlagChange>.Fail(new Error(FlagAccess.ResponseFlagsAnswer,
                "Answer permission is required to include an initial answer.", ErrorType.Security));
        }

        if (!await responses.AnyAsync(x => x.Id == request.ResponseId && x.Flags.Any(), ct))
            return Result<ResponseFlagChange>.Fail(new Error(
                "ResponseFlags.ResponseNotFound", "Flagged response not found.", ErrorType.NotFound));

        var flag = SurveyResponseFlag.CreateForFollowUp(request.ResponseId, request.Type, request.Brief, actor.Value);
        if (request.Answer is not null)
            flag.WriteAnswer(request.Answer, actor.Value);

        await flags.AddAsync(flag, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<ResponseFlagChange>.Ok(new(flag.Id, flag.SurveyResponseId));
    }
}

internal sealed class EditResponseFlagHandler(
    FlagOfficerAccess access,
    IWriteReadRepository<SurveyResponseFlag> flags,
    IUnitOfWork unitOfWork) :
    ICommandHandler<UpdateResponseFlagBriefCommand, ResponseFlagChange>,
    ICommandHandler<WriteResponseFlagAnswerCommand, ResponseFlagChange>
{
    public Task<Result<ResponseFlagChange>> Handle(UpdateResponseFlagBriefCommand request, CancellationToken ct)
        => UpdateAsync(request.FlagId, FlagAccess.ResponseFlagsEditBrief,
            (flag, userId) => flag.UpdateBrief(request.Brief, userId), ct);

    public Task<Result<ResponseFlagChange>> Handle(WriteResponseFlagAnswerCommand request, CancellationToken ct)
        => UpdateAsync(request.FlagId, FlagAccess.ResponseFlagsAnswer,
            (flag, userId) => flag.WriteAnswer(request.Answer, userId), ct);

    private async Task<Result<ResponseFlagChange>> UpdateAsync(
        Guid flagId, string permission, Action<SurveyResponseFlag, Guid> update, CancellationToken ct)
    {
        var actor = await access.RequireAsync(permission, ct);
        if (actor.IsFailure)
            return Result<ResponseFlagChange>.Fail(actor.Errors);

        var flag = await flags.GetByPropertyTrackedAsync(x => x.Id == flagId, ct);
        if (flag is null)
            return Result<ResponseFlagChange>.Fail(ResponseFlagErrors.FlagNotFound);

        update(flag, actor.Value);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<ResponseFlagChange>.Ok(new(flag.Id, flag.SurveyResponseId));
    }
}

internal static class ResponseFlagErrors
{
    public static readonly Error FlagNotFound = new("ResponseFlags.NotFound", "Response flag not found.", ErrorType.NotFound);
}
