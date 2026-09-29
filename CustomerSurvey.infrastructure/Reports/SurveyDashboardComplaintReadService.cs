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

        if (request.IncludeInternal)
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
                ComplaintId = x.Id,
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

        if (request.IncludeAnonymous)
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
                ComplaintId = x.Id,
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

        return result ?? _surveyAnswerRepository.Query()
            .Where(_ => false)
            .Select(x => new SurveyDashboardComplaintReadItem());
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
