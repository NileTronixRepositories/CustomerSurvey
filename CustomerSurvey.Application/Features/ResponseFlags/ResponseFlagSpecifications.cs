using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.ResponseFlags;

internal sealed class FlaggedResponsesSpec : Specification<SurveyResponse, FlaggedResponseSummary>
{
    public FlaggedResponsesSpec(GetFlaggedResponsesQuery request)
    {
        var type = request.Type;
        var answered = request.Answered;
        var search = request.SearchText?.Trim();
        var searchFlagText = !string.IsNullOrEmpty(search);
        AddCriteria(x => x.Flags.Any(flag =>
            (!type.HasValue || flag.Type == type) &&
            (!answered.HasValue || (flag.Answer != null) == answered) &&
            (!searchFlagText ||
                (flag.Brief != null && flag.Brief.Contains(search!)) ||
                (flag.Answer != null && flag.Answer.Contains(search!)) ||
                x.Template.NameEn.Contains(search!) || (x.Template.NameAr != null && x.Template.NameAr.Contains(search!)) ||
                x.Template.Branch.NameEn.Contains(search!) || (x.Template.Branch.NameAr != null && x.Template.Branch.NameAr.Contains(search!)) ||
                x.Operator.ApplicationUser.NameEn.Contains(search!) ||
                (x.Operator.ApplicationUser.NameAr != null && x.Operator.ApplicationUser.NameAr.Contains(search!)))));

        if (request.From is { } from)
        {
            var start = from.ToDateTime(TimeOnly.MinValue);
            AddCriteria(x => x.SubmittedOnUtc >= start);
        }
        if (request.To is { } to)
        {
            var end = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
            AddCriteria(x => x.SubmittedOnUtc < end);
        }

        if (request.OrderSort == OrderSort.Newest)
        {
            AddOrderByDescending(x => x.SubmittedOnUtc);
            AddOrderByDescending(x => x.Id);
        }
        else
        {
            AddOrderBy(x => x.SubmittedOnUtc);
            AddOrderBy(x => x.Id);
        }

        EnableTotalCount();
        ApplyPaging(request.PageNumber, request.PageSize);
        Select(x => new FlaggedResponseSummary(x.Id, x.TemplateId, x.Template.NameEn, x.Template.NameAr,
            x.Template.BranchId, x.Template.Branch.NameEn, x.Template.Branch.NameAr, x.OperatorId,
            x.Operator.ApplicationUser.NameEn, x.Operator.ApplicationUser.NameAr, x.SubmittedOnUtc, x.Flags.Count));
    }
}

internal sealed class ResponseFlagDetailsSpec : Specification<SurveyResponseFlag, ResponseFlagDetails>
{
    public ResponseFlagDetailsSpec(Guid? flagId = null, Guid? responseId = null, Guid[]? flagIds = null)
    {
        if (flagId is { } id)
            AddCriteria(x => x.Id == id);
        if (responseId is { } owner)
            AddCriteria(x => x.SurveyResponseId == owner);
        if (flagIds is not null)
            AddCriteria(x => flagIds.Contains(x.Id));

        AddOrderBy(x => x.CreatedOnUtc);
        AddOrderBy(x => x.Id);
        Select(x => new ResponseFlagDetails(
            x.Id, x.SurveyResponseId, x.Type, x.Brief, x.Answer,
            new ResponseFlagActor(x.CreatedByApplicationUserId, x.CreatedBy.NameEn, x.CreatedBy.NameAr),
            x.CreatedOnUtc,
            x.BriefUpdatedBy == null ? null :
                new ResponseFlagActor(x.BriefUpdatedBy.Id, x.BriefUpdatedBy.NameEn, x.BriefUpdatedBy.NameAr),
            x.BriefUpdatedOnUtc,
            x.AnsweredBy == null ? null :
                new ResponseFlagActor(x.AnsweredBy.Id, x.AnsweredBy.NameEn, x.AnsweredBy.NameAr),
            x.AnsweredOnUtc));
    }
}
