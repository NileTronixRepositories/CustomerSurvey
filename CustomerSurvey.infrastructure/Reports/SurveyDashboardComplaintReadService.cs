using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.infrastructure.Reports;

internal sealed class SurveyDashboardComplaintReadService : ISurveyDashboardComplaintReadService
{
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerRepository;
    private readonly IWriteReadRepository<AnonymousSurveyAnswer> _anonymousSurveyAnswerRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseRepository;

    public SurveyDashboardComplaintReadService(
        IWriteReadRepository<SurveyAnswer> surveyAnswerRepository,
        IWriteReadRepository<AnonymousSurveyAnswer> anonymousSurveyAnswerRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseRepository,
        IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseRepository)
    {
        _surveyAnswerRepository = surveyAnswerRepository;
        _anonymousSurveyAnswerRepository = anonymousSurveyAnswerRepository;
        _surveyResponseRepository = surveyResponseRepository;
        _anonymousSurveyResponseRepository = anonymousSurveyResponseRepository;
    }

    public async Task<SurveyDashboardComplaintReadResult> ReadAsync(
        SurveyDashboardComplaintReadRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.IncludeInternal && (!request.IncludeAnonymous || request.FlagType != SurveyResponseFlagType.Complaint))
        {
            var responses = await BuildResponseQuery(request).CountAsync(cancellationToken);
            return new SurveyDashboardComplaintReadResult(0, 0, responses, [], []);
        }

        var complaintQuery = BuildComplaintQuery(request);
        var totalComplaints = await complaintQuery.CountAsync(cancellationToken);
        var responsesWithComplaints = await complaintQuery
            .Select(x => new { x.Source, x.ResponseId })
            .Distinct()
            .CountAsync(cancellationToken);

        var groups = await complaintQuery
            .GroupBy(x => new
            {
                x.TemplateId,
                x.TemplateKind,
                x.TemplateNameEn,
                x.TemplateNameAr,
                x.BranchId,
                x.BranchNameEn,
                x.BranchNameAr
            })
            .Select(group => new SurveyDashboardComplaintTemplateReadGroup
            {
                TemplateId = group.Key.TemplateId,
                TemplateKind = group.Key.TemplateKind,
                TemplateNameEn = group.Key.TemplateNameEn,
                TemplateNameAr = group.Key.TemplateNameAr,
                BranchId = group.Key.BranchId,
                BranchNameEn = group.Key.BranchNameEn,
                BranchNameAr = group.Key.BranchNameAr,
                TotalComplaints = group.Count(),
                ResponsesWithComplaints = group.Select(x => x.ResponseId).Distinct().Count()
            })
            .OrderByDescending(x => x.TotalComplaints)
            .ThenBy(x => x.TemplateNameEn)
            .ToArrayAsync(cancellationToken);

        var items = await complaintQuery
            .OrderByDescending(x => x.SubmittedOnUtc)
            .ThenByDescending(x => x.ComplaintId)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArrayAsync(cancellationToken);

        items = items.Select(item =>
        {
            var saved = SnapshotScoring.Read(item.SnapshotPayload)?.Questions.FirstOrDefault(x => x.QuestionId == item.QuestionId);
            return saved is null ? item : item with { QuestionTextEn = saved.TextEn, QuestionTextAr = saved.TextAr };
        }).ToArray();

        var totalResponses = await BuildResponseQuery(request).CountAsync(cancellationToken);

        return new SurveyDashboardComplaintReadResult(
            totalComplaints,
            responsesWithComplaints,
            totalResponses,
            groups,
            items);
    }

    private IQueryable<SurveyDashboardComplaintReadItem> BuildComplaintQuery(
        SurveyDashboardComplaintReadRequest request)
    {
        IQueryable<SurveyDashboardComplaintReadItem>? result = null;

        if (request.IncludeInternal && request.FlagType == SurveyResponseFlagType.Complaint)
        {
            var query = _surveyAnswerRepository.Query()
                .AsNoTracking()
                .Where(x =>
                    x.QuestionType == QuestionType.Complain &&
                    x.TextAnswer != null &&
                    x.TextAnswer.Trim() != string.Empty &&
                    x.SurveyResponse.SubmittedOnUtc >= request.FromUtc &&
                    x.SurveyResponse.SubmittedOnUtc < request.ToExclusiveUtc);

            if (request.BranchId.HasValue)
            {
                query = query.Where(x => x.SurveyResponse.Template.BranchId == request.BranchId.Value);
            }

            if (request.InternalTemplateId.HasValue)
            {
                query = query.Where(x => x.SurveyResponse.TemplateId == request.InternalTemplateId.Value);
            }

            result = query.Select(x => new SurveyDashboardComplaintReadItem
            {
                SnapshotPayload = x.SurveyResponse.Snapshot == null ? null : x.SurveyResponse.Snapshot.Payload,
                ComplaintId = x.Id,
                FlagId = null,
                FlagType = null,
                Brief = null,
                Answer = null,
                CreatedById = null,
                CreatedByNameEn = null,
                CreatedByNameAr = null,
                CreatedOnUtc = null,
                BriefUpdatedById = null,
                BriefUpdatedByNameEn = null,
                BriefUpdatedByNameAr = null,
                BriefUpdatedOnUtc = null,
                AnsweredById = null,
                AnsweredByNameEn = null,
                AnsweredByNameAr = null,
                AnsweredOnUtc = null,
                Source = SurveyDashboardSource.Internal,
                ResponseId = x.SurveyResponseId,
                TemplateId = x.SurveyResponse.TemplateId,
                TemplateKind = SurveyDashboardTemplateKind.Authorized,
                TemplateNameEn = x.SurveyResponse.Template.NameEn,
                TemplateNameAr = x.SurveyResponse.Template.NameAr,
                QuestionId = x.QuestionId,
                QuestionTextEn = x.Question.TextEn,
                QuestionTextAr = x.Question.TextAr,
                ComplaintText = x.TextAnswer!,
                SubmittedOnUtc = x.SurveyResponse.SubmittedOnUtc,
                BranchId = x.SurveyResponse.Template.BranchId,
                BranchNameEn = x.SurveyResponse.Template.Branch.NameEn,
                BranchNameAr = x.SurveyResponse.Template.Branch.NameAr,
                OperatorId = x.SurveyResponse.OperatorId,
                OperatorNameEn = x.SurveyResponse.Operator.ApplicationUser.NameEn,
                OperatorNameAr = x.SurveyResponse.Operator.ApplicationUser.NameAr
            });
        }

        if (request.IncludeAnonymous && request.FlagType == SurveyResponseFlagType.Complaint)
        {
            var query = _anonymousSurveyAnswerRepository.Query()
                .AsNoTracking()
                .Where(x =>
                    x.QuestionType == QuestionType.Complain &&
                    x.TextAnswer != null &&
                    x.TextAnswer.Trim() != string.Empty &&
                    x.AnonymousSurveyResponse.AnonymousTemplate.Scope == AnonymousTemplateScope.Branch &&
                    x.AnonymousSurveyResponse.AnonymousTemplate.BranchId != null &&
                    x.AnonymousSurveyResponse.SubmittedOnUtc >= request.FromUtc &&
                    x.AnonymousSurveyResponse.SubmittedOnUtc < request.ToExclusiveUtc);

            if (request.BranchId.HasValue)
            {
                query = query.Where(x =>
                    x.AnonymousSurveyResponse.AnonymousTemplate.BranchId == request.BranchId.Value);
            }

            if (request.AnonymousTemplateId.HasValue)
            {
                query = query.Where(x =>
                    x.AnonymousSurveyResponse.AnonymousTemplateId == request.AnonymousTemplateId.Value);
            }

            var projected = query.Select(x => new SurveyDashboardComplaintReadItem
            {
                SnapshotPayload = x.AnonymousSurveyResponse.Snapshot == null ? null : x.AnonymousSurveyResponse.Snapshot.Payload,
                ComplaintId = x.Id,
                FlagId = null,
                FlagType = null,
                Brief = null,
                Answer = null,
                CreatedById = null,
                CreatedByNameEn = null,
                CreatedByNameAr = null,
                CreatedOnUtc = null,
                BriefUpdatedById = null,
                BriefUpdatedByNameEn = null,
                BriefUpdatedByNameAr = null,
                BriefUpdatedOnUtc = null,
                AnsweredById = null,
                AnsweredByNameEn = null,
                AnsweredByNameAr = null,
                AnsweredOnUtc = null,
                Source = SurveyDashboardSource.Anonymous,
                ResponseId = x.AnonymousSurveyResponseId,
                TemplateId = x.AnonymousSurveyResponse.AnonymousTemplateId,
                TemplateKind = SurveyDashboardTemplateKind.Anonymous,
                TemplateNameEn = x.AnonymousSurveyResponse.AnonymousTemplate.NameEn,
                TemplateNameAr = x.AnonymousSurveyResponse.AnonymousTemplate.NameAr,
                QuestionId = x.QuestionId,
                QuestionTextEn = x.Question.TextEn,
                QuestionTextAr = x.Question.TextAr,
                ComplaintText = x.TextAnswer!,
                SubmittedOnUtc = x.AnonymousSurveyResponse.SubmittedOnUtc,
                BranchId = x.AnonymousSurveyResponse.AnonymousTemplate.BranchId!.Value,
                BranchNameEn = x.AnonymousSurveyResponse.AnonymousTemplate.Branch!.NameEn,
                BranchNameAr = x.AnonymousSurveyResponse.AnonymousTemplate.Branch.NameAr,
                OperatorId = null,
                OperatorNameEn = null,
                OperatorNameAr = null
            });

            result = result is null ? projected : result.Concat(projected);
        }

        if (request.IncludeInternal)
        {
            var flags = BuildFlagQuery(request);
            result = result is null ? flags : result.Concat(flags);
        }

        return result ?? _surveyAnswerRepository.Query()
            .Where(_ => false)
            .Select(x => new SurveyDashboardComplaintReadItem());
    }

    private IQueryable<SurveyDashboardComplaintReadItem> BuildFlagQuery(SurveyDashboardComplaintReadRequest request)
    {
        var responses = _surveyResponseRepository.Query().AsNoTracking()
            .Where(x => x.SubmittedOnUtc >= request.FromUtc && x.SubmittedOnUtc < request.ToExclusiveUtc);
        if (request.BranchId is { } branchId)
            responses = responses.Where(x => x.Template.BranchId == branchId);
        if (request.InternalTemplateId is { } templateId)
            responses = responses.Where(x => x.TemplateId == templateId);

        var flags = responses.SelectMany(x => x.Flags).Where(x => x.Type == request.FlagType);
        if (request.Answered is { } answered)
            flags = flags.Where(x => (x.Answer != null) == answered);
        if (request.FlagId is { } flagId)
            flags = flags.Where(x => x.Id == flagId);
        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var search = request.SearchText.Trim();
            flags = flags.Where(x => (x.Brief != null && x.Brief.Contains(search)) ||
                (x.Answer != null && x.Answer.Contains(search)));
        }

        return flags.Select(x => new SurveyDashboardComplaintReadItem
        {
            SnapshotPayload = null,
            ComplaintId = x.Id,
            FlagId = x.Id,
            FlagType = x.Type,
            Brief = x.Brief,
            Answer = x.Answer,
            CreatedById = x.CreatedByApplicationUserId,
            CreatedByNameEn = x.CreatedBy.NameEn,
            CreatedByNameAr = x.CreatedBy.NameAr,
            CreatedOnUtc = x.CreatedOnUtc,
            BriefUpdatedById = x.BriefUpdatedByApplicationUserId,
            BriefUpdatedByNameEn = x.BriefUpdatedBy == null ? null : x.BriefUpdatedBy.NameEn,
            BriefUpdatedByNameAr = x.BriefUpdatedBy == null ? null : x.BriefUpdatedBy.NameAr,
            BriefUpdatedOnUtc = x.BriefUpdatedOnUtc,
            AnsweredById = x.AnsweredByApplicationUserId,
            AnsweredByNameEn = x.AnsweredBy == null ? null : x.AnsweredBy.NameEn,
            AnsweredByNameAr = x.AnsweredBy == null ? null : x.AnsweredBy.NameAr,
            AnsweredOnUtc = x.AnsweredOnUtc,
            Source = SurveyDashboardSource.Internal,
            ResponseId = x.SurveyResponseId,
            TemplateId = x.SurveyResponse.TemplateId,
            TemplateKind = SurveyDashboardTemplateKind.Authorized,
            TemplateNameEn = x.SurveyResponse.Template.NameEn,
            TemplateNameAr = x.SurveyResponse.Template.NameAr,
            QuestionId = null,
            QuestionTextEn = string.Empty,
            QuestionTextAr = null,
            ComplaintText = x.Brief,
            SubmittedOnUtc = x.SurveyResponse.SubmittedOnUtc,
            BranchId = x.SurveyResponse.Template.BranchId,
            BranchNameEn = x.SurveyResponse.Template.Branch.NameEn,
            BranchNameAr = x.SurveyResponse.Template.Branch.NameAr,
            OperatorId = x.SurveyResponse.OperatorId,
            OperatorNameEn = x.SurveyResponse.Operator.ApplicationUser.NameEn,
            OperatorNameAr = x.SurveyResponse.Operator.ApplicationUser.NameAr
        });
    }

    private IQueryable<ComplaintResponseKey> BuildResponseQuery(SurveyDashboardComplaintReadRequest request)
    {
        IQueryable<ComplaintResponseKey>? result = null;

        if (request.IncludeInternal)
        {
            var query = _surveyResponseRepository.Query()
                .AsNoTracking()
                .Where(x => x.SubmittedOnUtc >= request.FromUtc && x.SubmittedOnUtc < request.ToExclusiveUtc);

            if (request.BranchId.HasValue)
            {
                query = query.Where(x => x.Template.BranchId == request.BranchId.Value);
            }

            if (request.InternalTemplateId.HasValue)
            {
                query = query.Where(x => x.TemplateId == request.InternalTemplateId.Value);
            }

            result = query.Select(x => new ComplaintResponseKey
            {
                Source = SurveyDashboardSource.Internal,
                ResponseId = x.Id
            });
        }

        if (request.IncludeAnonymous)
        {
            var query = _anonymousSurveyResponseRepository.Query()
                .AsNoTracking()
                .Where(x =>
                    x.AnonymousTemplate.Scope == AnonymousTemplateScope.Branch &&
                    x.AnonymousTemplate.BranchId != null &&
                    x.SubmittedOnUtc >= request.FromUtc &&
                    x.SubmittedOnUtc < request.ToExclusiveUtc);

            if (request.BranchId.HasValue)
            {
                query = query.Where(x => x.AnonymousTemplate.BranchId == request.BranchId.Value);
            }

            if (request.AnonymousTemplateId.HasValue)
            {
                query = query.Where(x => x.AnonymousTemplateId == request.AnonymousTemplateId.Value);
            }

            var projected = query.Select(x => new ComplaintResponseKey
            {
                Source = SurveyDashboardSource.Anonymous,
                ResponseId = x.Id
            });

            result = result is null ? projected : result.Concat(projected);
        }

        return result ?? _surveyResponseRepository.Query()
            .Where(_ => false)
            .Select(x => new ComplaintResponseKey());
    }

    private sealed record ComplaintResponseKey
    {
        public SurveyDashboardSource Source { get; init; }
        public Guid ResponseId { get; init; }
    }
}
