using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardComplaints;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardQuestionGroups;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class SurveyDashboardAnalyticsEndpointTests
{
    [Fact]
    public void ComplaintValidator_RejectsAmbiguousTemplatesInvalidDatesAndInvalidPagination()
    {
        var result = new GetSurveyDashboardComplaintsQueryValidator().Validate(
            new GetSurveyDashboardComplaintsQuery
            {
                TemplateId = Guid.NewGuid(),
                AnonymousTemplateId = Guid.NewGuid(),
                From = new DateTime(2026, 2, 2),
                To = new DateTime(2026, 2, 1),
                PageNumber = 0,
                PageSize = 101
            });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(GetSurveyDashboardComplaintsQuery.PageNumber));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(GetSurveyDashboardComplaintsQuery.PageSize));
        Assert.Equal(2, result.Errors.Count(x => x.PropertyName == string.Empty));
    }

    [Fact]
    public void QuestionGroupValidator_RejectsAmbiguousTemplatesAndInvalidScoreMode()
    {
        var result = new GetSurveyDashboardQuestionGroupsQueryValidator().Validate(
            new GetSurveyDashboardQuestionGroupsQuery
            {
                TemplateId = Guid.NewGuid(),
                AnonymousTemplateId = Guid.NewGuid(),
                ScoreCalculationMode = (ScoreCalculationMode)999
            });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == string.Empty);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(GetSurveyDashboardQuestionGroupsQuery.ScoreCalculationMode));
    }

    [Fact]
    public async Task ComplaintHandler_MapsTotalsPaginationGroupsAndBranchAccessibleNavigation()
    {
        var branchId = Guid.NewGuid();
        var internalTemplateId = Guid.NewGuid();
        var anonymousTemplateId = Guid.NewGuid();
        var internalResponseId = Guid.NewGuid();
        var anonymousResponseId = Guid.NewGuid();
        var context = BranchContext(branchId);
        var readService = new StubComplaintReadService(new SurveyDashboardComplaintReadResult(
            TotalComplaints: 3,
            ResponsesWithComplaints: 2,
            TotalResponses: 8,
            TemplateGroups: new[]
            {
                Group(internalTemplateId, SurveyDashboardTemplateKind.Authorized, branchId, 2, 1),
                Group(anonymousTemplateId, SurveyDashboardTemplateKind.Anonymous, branchId, 1, 1)
            },
            PageItems: new[]
            {
                Item(internalResponseId, internalTemplateId, SurveyDashboardSource.Internal,
                    SurveyDashboardTemplateKind.Authorized, branchId),
                Item(anonymousResponseId, anonymousTemplateId, SurveyDashboardSource.Anonymous,
                    SurveyDashboardTemplateKind.Anonymous, branchId)
            }));
        var handler = new GetSurveyDashboardComplaintsQueryHandler(
            new StubDashboardResolver(context),
            readService);

        var result = await handler.Handle(new GetSurveyDashboardComplaintsQuery
        {
            PageNumber = 2,
            PageSize = 2
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.TotalComplaints);
        Assert.Equal(2, result.Value.ResponsesWithComplaints);
        Assert.Equal(8, result.Value.TotalResponses);
        Assert.Equal(25m, result.Value.ComplaintRate);
        Assert.Equal(2, result.Value.Page.CurrentPage);
        Assert.Equal(3, result.Value.Page.TotalItems);
        Assert.Equal(2, result.Value.Page.Data.Count);
        Assert.Equal(2, result.Value.TemplateGroups.Count);
        Assert.All(result.Value.TemplateGroups, group => Assert.Single(group.Complaints));
        Assert.Equal(
            $"/api/reports/branch-responses/{internalResponseId}",
            result.Value.Page.Data.Single(x => x.Source == SurveyDashboardSource.Internal).DetailsNavigation.Path);
        Assert.Equal(
            $"/api/anonymous-templates/{anonymousTemplateId}/responses/{anonymousResponseId}",
            result.Value.Page.Data.Single(x => x.Source == SurveyDashboardSource.Anonymous).DetailsNavigation.Path);

        Assert.NotNull(readService.LastRequest);
        Assert.Equal(branchId, readService.LastRequest!.BranchId);
        Assert.Equal(2, readService.LastRequest.PageNumber);
        Assert.Equal(2, readService.LastRequest.PageSize);
        Assert.Equal(new DateTime(2026, 1, 1), readService.LastRequest.FromUtc);
        Assert.Equal(new DateTime(2026, 2, 1), readService.LastRequest.ToExclusiveUtc);
    }

    [Fact]
    public async Task ComplaintHandler_UsesSystemDetailsForSuperAdminInternalResponses()
    {
        var responseId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var readService = new StubComplaintReadService(new SurveyDashboardComplaintReadResult(
            1,
            1,
            1,
            new[] { Group(templateId, SurveyDashboardTemplateKind.Authorized, branchId, 1, 1) },
            new[]
            {
                Item(responseId, templateId, SurveyDashboardSource.Internal,
                    SurveyDashboardTemplateKind.Authorized, branchId)
            }));
        var handler = new GetSurveyDashboardComplaintsQueryHandler(
            new StubDashboardResolver(SuperAdminContext()),
            readService);

        var result = await handler.Handle(
            new GetSurveyDashboardComplaintsQuery(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            $"/api/reports/system-responses/{responseId}",
            Assert.Single(result.Value.Page.Data).DetailsNavigation.Path);
    }

    private static ResolvedSurveyDashboardRequest BranchContext(Guid branchId)
    {
        var branch = new SurveyDashboardBranchRow
        {
            BranchId = branchId,
            BranchNameEn = "Cairo",
            IsActive = true
        };

        return new ResolvedSurveyDashboardRequest(
            CurrentSurveyDashboardActor.BranchScoped("BranchAdmin", branch),
            new ResolvedSurveyDashboardScope(
                branchId, "Cairo", null, "CurrentBranch", new[] { branch }),
            new ResolvedSurveyDashboardPeriod(
                new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), false),
            null,
            SurveyDashboardSource.All,
            true,
            true,
            null,
            null);
    }

    private static ResolvedSurveyDashboardRequest SuperAdminContext()
        => new(
            CurrentSurveyDashboardActor.SuperAdmin(),
            new ResolvedSurveyDashboardScope(null, null, null, "AllBranches",
                Array.Empty<SurveyDashboardBranchRow>()),
            new ResolvedSurveyDashboardPeriod(
                new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), false),
            null,
            SurveyDashboardSource.Internal,
            true,
            false,
            null,
            null);

    private static SurveyDashboardComplaintTemplateReadGroup Group(
        Guid templateId,
        SurveyDashboardTemplateKind kind,
        Guid branchId,
        int totalComplaints,
        int responsesWithComplaints)
        => new()
        {
            TemplateId = templateId,
            TemplateKind = kind,
            TemplateNameEn = kind.ToString(),
            BranchId = branchId,
            BranchNameEn = "Cairo",
            TotalComplaints = totalComplaints,
            ResponsesWithComplaints = responsesWithComplaints
        };

    private static SurveyDashboardComplaintReadItem Item(
        Guid responseId,
        Guid templateId,
        SurveyDashboardSource source,
        SurveyDashboardTemplateKind kind,
        Guid branchId)
        => new()
        {
            ComplaintId = Guid.NewGuid(),
            ResponseId = responseId,
            TemplateId = templateId,
            TemplateKind = kind,
            TemplateNameEn = kind.ToString(),
            QuestionId = Guid.NewGuid(),
            QuestionTextEn = "Complaint question",
            ComplaintText = "A valid complaint",
            SubmittedOnUtc = new DateTime(2026, 1, 10),
            BranchId = branchId,
            BranchNameEn = "Cairo",
            Source = source
        };

    private sealed class StubDashboardResolver : ISurveyDashboardRequestResolver
    {
        private readonly ResolvedSurveyDashboardRequest _context;

        public StubDashboardResolver(ResolvedSurveyDashboardRequest context)
        {
            _context = context;
        }

        public Task<Result<ResolvedSurveyDashboardRequest>> ResolveAsync(
            ISurveyDashboardFilterRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(Result<ResolvedSurveyDashboardRequest>.Ok(_context));
    }

    private sealed class StubComplaintReadService : ISurveyDashboardComplaintReadService
    {
        private readonly SurveyDashboardComplaintReadResult _result;

        public StubComplaintReadService(SurveyDashboardComplaintReadResult result)
        {
            _result = result;
        }

        public SurveyDashboardComplaintReadRequest? LastRequest { get; private set; }

        public Task<SurveyDashboardComplaintReadResult> ReadAsync(
            SurveyDashboardComplaintReadRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_result);
        }
    }
}
