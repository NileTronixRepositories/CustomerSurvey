using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.ResponseFlags;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.Application.Features.Reports.Shared;

internal sealed class AuthorizedFeedbackItems(
    IWriteReadRepository<SurveyResponse> responses,
    IWriteReadRepository<SurveyResponseFlag> flags)
{
    public async Task<Pagination<T>> PageAsync<T>(
        Specification<SurveyResponse, T> responseSpec,
        SearchParameters paging,
        SurveyResponseFlagType type,
        Guid? questionId,
        CancellationToken ct,
        Func<T, Guid> responseId,
        Func<T, FeedbackItem, ResponseFlagDetails?, T> map)
    {
        var scoped = responses.Query().AsNoTracking().Where(responseSpec.Criteria);
        var flagItems = scoped.SelectMany(x => x.Flags)
            .Where(x => x.Type == type && !questionId.HasValue)
            .Select(x => new FeedbackItem
            {
                ItemId = x.Id,
                ResponseId = x.SurveyResponseId,
                SubmittedOnUtc = x.SurveyResponse.SubmittedOnUtc,
                QuestionId = null,
                FlagId = x.Id,
                Text = x.Brief
            });

        IQueryable<FeedbackItem> items = flagItems;
        if (type == SurveyResponseFlagType.Complaint)
        {
            var questionItems = scoped.SelectMany(x => x.Answers)
                .Where(x => x.QuestionType == QuestionType.Complain && x.TextAnswer != null &&
                    x.TextAnswer.Trim() != string.Empty && (!questionId.HasValue || x.QuestionId == questionId))
                .Select(x => new FeedbackItem
                {
                    ItemId = x.Id,
                    ResponseId = x.SurveyResponseId,
                    SubmittedOnUtc = x.SurveyResponse.SubmittedOnUtc,
                    QuestionId = x.QuestionId,
                    FlagId = null,
                    Text = x.TextAnswer
                });
            items = questionItems.Concat(flagItems);
        }

        var count = await items.CountAsync(ct);
        var page = await items.OrderByDescending(x => x.SubmittedOnUtc).ThenBy(x => x.ItemId)
            .Skip((paging.PageNumber - 1) * paging.PageSize).Take(paging.PageSize).ToListAsync(ct);
        if (page.Count == 0)
            return new Pagination<T>(paging.PageNumber, paging.PageSize, count, []);

        var responseIds = page.Select(x => x.ResponseId).Distinct().ToArray();
        var responseItems = await scoped.Where(x => responseIds.Contains(x.Id))
            .Select(responseSpec.Selector).ToDictionaryAsync(responseId, ct);

        var flagIds = page.Where(x => x.FlagId.HasValue).Select(x => x.ItemId).ToArray();
        var flagDetails = flagIds.Length == 0
            ? new Dictionary<Guid, ResponseFlagDetails>()
            : (await flags.ListAsync(new ResponseFlagDetailsSpec(flagIds: flagIds), ct)).ToDictionary(x => x.FlagId);

        return new Pagination<T>(paging.PageNumber, paging.PageSize, count,
            page.Where(x => responseItems.ContainsKey(x.ResponseId))
                .Select(x => map(responseItems[x.ResponseId], x, flagDetails.GetValueOrDefault(x.ItemId))).ToArray());
    }

    internal sealed record FeedbackItem
    {
        public Guid ItemId { get; init; }
        public Guid ResponseId { get; init; }
        public DateTime SubmittedOnUtc { get; init; }
        public Guid? QuestionId { get; init; }
        public Guid? FlagId { get; init; }
        public string? Text { get; init; }
    }
}
