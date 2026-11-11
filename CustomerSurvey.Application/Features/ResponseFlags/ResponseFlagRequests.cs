using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.ResponseFlags;

public sealed class GetFlaggedResponsesQuery : SearchParameters, IQuery<Pagination<FlaggedResponseSummary>>
{
    public SurveyResponseFlagType? Type { get; init; }
    public bool? Answered { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public sealed record GetFlaggedResponseQuery(Guid ResponseId) : IQuery<FlaggedResponseDetails>;
public sealed record GetResponseFlagQuery(Guid FlagId) : IQuery<ResponseFlagDetails>;
public sealed record CreateResponseFlagCommand(Guid ResponseId, SurveyResponseFlagType Type, string Brief, string? Answer)
    : ICommand<ResponseFlagChange>;
public sealed record UpdateResponseFlagBriefCommand(Guid FlagId, string Brief) : ICommand<ResponseFlagChange>;
public sealed record WriteResponseFlagAnswerCommand(Guid FlagId, string Answer) : ICommand<ResponseFlagChange>;

public sealed record FlaggedResponseSummary(
    Guid ResponseId, Guid TemplateId, string TemplateNameEn, string? TemplateNameAr,
    Guid BranchId, string BranchNameEn, string? BranchNameAr, Guid OperatorId,
    string OperatorNameEn, string? OperatorNameAr, DateTime SubmittedOnUtc, int FlagsCount);

public sealed record FlaggedResponseDetails(
    GetSystemSurveyResponseDetailsResponse Response, IReadOnlyList<ResponseFlagDetails> Flags);

public sealed record ResponseFlagChange(Guid FlagId, Guid ResponseId);

public sealed record ResponseFlagActor(Guid ApplicationUserId, string NameEn, string? NameAr);

public sealed record ResponseFlagDetails(
    Guid FlagId, Guid ResponseId, SurveyResponseFlagType Type, string? Brief, string? Answer,
    ResponseFlagActor CreatedBy, DateTime CreatedOnUtc,
    ResponseFlagActor? BriefUpdatedBy, DateTime? BriefUpdatedOnUtc,
    ResponseFlagActor? AnsweredBy, DateTime? AnsweredOnUtc);
