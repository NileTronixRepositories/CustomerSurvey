using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.ResponseFlags;

internal sealed class ResponseFlagQueryHandlers(
    FlagOfficerAccess access,
    IWriteReadRepository<SurveyResponse> responses,
    IWriteReadRepository<SurveyResponseFlag> flags,
    AuthorizedResponseDetailsReader responseReader) :
    IQueryHandler<GetFlaggedResponsesQuery, Pagination<FlaggedResponseSummary>>,
    IQueryHandler<GetFlaggedResponseQuery, FlaggedResponseDetails>,
    IQueryHandler<GetResponseFlagQuery, ResponseFlagDetails>
{
    public async Task<Result<Pagination<FlaggedResponseSummary>>> Handle(GetFlaggedResponsesQuery request, CancellationToken ct)
    {
        var actor = await access.RequireAsync(FlagAccess.ResponseFlagsView, ct);
        if (actor.IsFailure)
            return Result<Pagination<FlaggedResponseSummary>>.Fail(actor.Errors);

        var (items, count) = await responses.ListWithCountAsync(new FlaggedResponsesSpec(request), ct);
        return Result<Pagination<FlaggedResponseSummary>>.Ok(new(request.PageNumber, request.PageSize, count, items));
    }

    public async Task<Result<FlaggedResponseDetails>> Handle(GetFlaggedResponseQuery request, CancellationToken ct)
    {
        var actor = await access.RequireAsync(FlagAccess.ResponseFlagsView, ct);
        if (actor.IsFailure)
            return Result<FlaggedResponseDetails>.Fail(actor.Errors);

        var response = await responseReader.ReadAsync(request.ResponseId, requireFlags: true, ct);
        if (response.IsFailure)
            return Result<FlaggedResponseDetails>.Fail(response.Errors);

        var items = await flags.ListAsync(new ResponseFlagDetailsSpec(responseId: request.ResponseId), ct);
        return Result<FlaggedResponseDetails>.Ok(new(response.Value, items));
    }

    public async Task<Result<ResponseFlagDetails>> Handle(GetResponseFlagQuery request, CancellationToken ct)
    {
        var actor = await access.RequireAsync(FlagAccess.ResponseFlagsView, ct);
        if (actor.IsFailure)
            return Result<ResponseFlagDetails>.Fail(actor.Errors);

        var flag = await flags.FirstOrDefaultAsync(new ResponseFlagDetailsSpec(flagId: request.FlagId), ct);
        return flag is null
            ? Result<ResponseFlagDetails>.Fail(ResponseFlagErrors.FlagNotFound)
            : Result<ResponseFlagDetails>.Ok(flag);
    }
}
