using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.MultiTenancy;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Auth.Command.Login;
using CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword;
using CustomerSurvey.Application.Features.FlagOfficers;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardComplaints;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Application.Features.ResponseFlags;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Application.Options;
using CustomerSurvey.Application.Shared;
using CustomerSurvey.Application.Shared.Dto;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.infrastructure.Persistence;
using CustomerSurvey.infrastructure.Reports;
using CustomerSurvey.infrastructure.Repositories;
using CustomerSurvey.infrastructure.Seeders;
using CustomerSurvey.Tests.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace CustomerSurvey.Tests.ResponseFlags;

public sealed class ResponseFlagsSqlTests
{
    private const string Password = "TestPassword123";

    [SqlHistoryFact]
    public async Task SubmissionAndFollowUpEnforceIndependentPermissionsAndPreserveOriginalResponse()
    {
        await using var store = await FlagStore.CreateAsync();
        var db = store.Db;
        var actor = store.Admin.Id;
        var user = NewUser(UserType.Operator, actor);
        var op = Operator.Create(user.Id, store.Department.Id, actor);
        var firstBranch = Branch.Create("First", null, "FIRST", null, actor);
        var secondBranch = Branch.Create("Second", null, "SECOND", null, actor);
        var first = Template.Create(firstBranch.Id, "First", null, null, DateTime.UtcNow.AddDays(-1), null, actor);
        var second = Template.Create(secondBranch.Id, "Second", null, null, DateTime.UtcNow.AddDays(-1), null, actor);
        var group = QuestionGroup.Create(firstBranch.Id, "Test", null, actor);
        var question = Question.Create(firstBranch.Id, group.Id, "Complaint", null, QuestionType.Complain, actor);
        var operatorRole = await db.Set<Role>().SingleAsync(x => x.Name == SeedConstants.RoleNames.Operator);
        db.AddRange(user, UserRole.Create(user.Id, operatorRole.Id, actor), op, firstBranch, secondBranch, first, second, group, question,
            OperatorTemplate.Create(op.Id, first.Id, actor), TemplateQuestion.Create(first.Id, question.Id, 1, actor));
        await db.SaveChangesAsync();
        var anonymous = AnonymousTemplate.CreateBranchTemplate(firstBranch.Id, "Anonymous", null, null, DateTime.UtcNow.AddDays(-1), null, actor);
        var anonLink = AnonymousTemplateQuestion.Create(anonymous.Id, question.Id, 1, actor);
        db.AddRange(anonymous, anonLink);
        await db.SaveChangesAsync();
        var anonymousResponse = AnonymousSurveyResponse.Create(anonymous.Id, 0, 0, 0);
        var anonymousComplaint = new string('a', 4000);
        anonymousResponse.AddAnswer(AnonymousSurveyAnswer.CreateComplain(anonymousResponse.Id, anonLink.Id, question.Id, anonymousComplaint));
        db.Add(anonymousResponse);
        await db.SaveChangesAsync();

        var submission = new SubmitOperatorTemplateResponseCommandHandler(store.Read<Operator>(),
            store.Read<OperatorTemplate>(), store.Read<TemplateQuestion>(), store.Read<TemplateCustomInput>(),
            store.Read<QuestionOption>(), store.Read<TemplateQuestionCondition>(), store.Write<SurveyResponse>(),
            new SubmissionMedia(), new TestCurrentUser(user.Id), store.Unit, store.Permissions);
        var authorizedComplaint = new string('q', 4000);
        var request = new SubmitOperatorTemplateResponseCommand
        {
            TemplateId = first.Id,
            Answers = [new SubmitOperatorTemplateAnswerCommandItem { QuestionId = question.Id, TextAnswer = authorizedComplaint }]
        };
        var noFlags = await submission.Handle(request, default);
        Assert.True(noFlags.IsSuccess, string.Join(", ", noFlags.Errors));

        var withFlags = request with
        {
            Flags =
            [
                new(SurveyResponseFlagType.Complaint, null),
                new(SurveyResponseFlagType.Complaint, "  "),
                new(SurveyResponseFlagType.Suggestion, null),
                new(SurveyResponseFlagType.Suggestion, "One suggestion"),
                new(SurveyResponseFlagType.Comment, "")
            ]
        };
        Assert.True((await submission.Handle(withFlags, default)).IsFailure);
        Assert.Empty(await db.Set<SurveyResponseFlag>().ToListAsync());

        var operatorAccess = new SetOperatorFlagSubmissionHandler(store.Scope(actor), store.Read<Operator>(), store.Read<Role>(),
            store.Read<UserRole>(), store.Write<UserRole>(), store.Unit);
        Assert.True((await operatorAccess.Handle(new SetOperatorFlagSubmissionCommand(op.Id, true), default)).IsSuccess);
        var submitted = await submission.Handle(withFlags, default);
        Assert.True(submitted.IsSuccess, string.Join(", ", submitted.Errors));
        var responseId = submitted.Value.SurveyResponseId;
        var snapshot = await db.Set<ResponseSnapshot>().SingleAsync(x => x.SurveyResponseId == responseId);
        var savedFlags = await db.Set<SurveyResponseFlag>().Where(x => x.SurveyResponseId == responseId).ToListAsync();
        Assert.Equal(5, savedFlags.Count);
        Assert.Equal(4, savedFlags.Count(x => x.Brief is null));
        Assert.All(savedFlags, flag =>
        {
            Assert.Equal(user.Id, flag.CreatedByApplicationUserId);
            Assert.Null(flag.Answer);
        });
        var originalFlag = savedFlags.First(x => x.Type == SurveyResponseFlagType.Complaint);

        var crossBranch = SurveyResponse.Create(op.Id, second.Id, user.Id, 0, 0, 0);
        crossBranch.AddSubmittedFlag(SurveyResponseFlagType.Complaint, null);
        var unflagged = SurveyResponse.Create(op.Id, second.Id, user.Id, 0, 0, 0);
        db.AddRange(crossBranch, unflagged);
        await db.SaveChangesAsync();

        var officer = (await store.CreateOfficer(actor, store.OtherDepartment.Id)).Value;
        var officerAccess = store.Access(officer.ApplicationUserId);
        var queries = store.Queries(officerAccess);
        var edit = new EditResponseFlagHandler(officerAccess, store.Read<SurveyResponseFlag>(), store.Unit);
        var create = new CreateResponseFlagHandler(officerAccess, store.Permissions, store.Read<SurveyResponse>(),
            store.Write<SurveyResponseFlag>(), store.Unit);
        Assert.True((await queries.Handle(new GetFlaggedResponsesQuery(), default)).IsFailure);
        Assert.True((await edit.Handle(new WriteResponseFlagAnswerCommand(originalFlag.Id, "Answer"), default)).IsFailure);
        Assert.True((await edit.Handle(new UpdateResponseFlagBriefCommand(originalFlag.Id, "Brief"), default)).IsFailure);
        Assert.True((await create.Handle(new CreateResponseFlagCommand(responseId, SurveyResponseFlagType.Comment, "Brief", null), default)).IsFailure);

        var opAccess = store.Access(user.Id);
        Assert.True((await store.Queries(opAccess).Handle(new GetResponseFlagQuery(originalFlag.Id), default)).IsFailure);
        var operatorEdit = new EditResponseFlagHandler(opAccess, store.Read<SurveyResponseFlag>(), store.Unit);
        Assert.True((await operatorEdit.Handle(new WriteResponseFlagAnswerCommand(originalFlag.Id, "Answer"), default)).IsFailure);
        Assert.True((await operatorEdit.Handle(new UpdateResponseFlagBriefCommand(originalFlag.Id, "Brief"), default)).IsFailure);

        await store.Grant(officer, FlagAccess.ViewRole, FlagAccess.CreateRole, FlagAccess.AnswerRole);
        var list = (await queries.Handle(new GetFlaggedResponsesQuery(), default)).Value;
        Assert.Equal(2, list.TotalItems);
        Assert.Equal(2, list.Data.Select(x => x.BranchId).Distinct().Count());
        var unanswered = (await queries.Handle(new GetFlaggedResponsesQuery { Type = SurveyResponseFlagType.Suggestion, Answered = false }, default)).Value;
        Assert.Equal(responseId, Assert.Single(unanswered.Data).ResponseId);
        Assert.True((await queries.Handle(new GetFlaggedResponseQuery(noFlags.Value.SurveyResponseId), default)).IsFailure);
        var officerSubmission = new SubmitOperatorTemplateResponseCommandHandler(store.Read<Operator>(),
            store.Read<OperatorTemplate>(), store.Read<TemplateQuestion>(), store.Read<TemplateCustomInput>(),
            store.Read<QuestionOption>(), store.Read<TemplateQuestionCondition>(), store.Write<SurveyResponse>(),
            new SubmissionMedia(), new TestCurrentUser(officer.ApplicationUserId), store.Unit, store.Permissions);
        Assert.True((await officerSubmission.Handle(request, default)).IsFailure);
        var details = (await queries.Handle(new GetFlaggedResponseQuery(responseId), default)).Value;
        Assert.Equal(5, details.Flags.Count);
        Assert.Equal(op.Id, details.Response.Operator.OperatorId);
        Assert.NotNull(details.Response.HistoricalSurvey);

        Assert.True((await edit.Handle(new WriteResponseFlagAnswerCommand(originalFlag.Id, "First answer"), default)).IsSuccess);
        var fullAnswer = new string('r', SurveyResponseFlag.MaxAnswerLength);
        Assert.True((await edit.Handle(new WriteResponseFlagAnswerCommand(originalFlag.Id, fullAnswer), default)).IsSuccess);
        var answered = (await queries.Handle(new GetResponseFlagQuery(originalFlag.Id), default)).Value;
        Assert.Null(answered.Brief);
        Assert.Equal(fullAnswer, answered.Answer);
        Assert.Equal(officer.ApplicationUserId, answered.AnsweredBy!.ApplicationUserId);
        Assert.NotNull(answered.AnsweredOnUtc);
        Assert.Equal(user.Id, answered.CreatedBy.ApplicationUserId);
        Assert.True((await edit.Handle(new UpdateResponseFlagBriefCommand(originalFlag.Id, "Brief"), default)).IsFailure);
        Assert.True((await create.Handle(new CreateResponseFlagCommand(noFlags.Value.SurveyResponseId, SurveyResponseFlagType.Comment, "Brief", null), default)).IsFailure);
        Assert.True((await create.Handle(new CreateResponseFlagCommand(anonymousResponse.Id, SurveyResponseFlagType.Comment, "Brief", null), default)).IsFailure);
        Assert.True((await create.Handle(new CreateResponseFlagCommand(responseId, SurveyResponseFlagType.Comment, "Another comment", "Initial answer"), default)).IsSuccess);

        await store.Grant(officer, FlagAccess.ViewRole, FlagAccess.CreateRole, FlagAccess.EditBriefRole);
        Assert.True((await create.Handle(new CreateResponseFlagCommand(responseId, SurveyResponseFlagType.Comment, "Brief", "Unauthorized"), default)).IsFailure);
        var fullBrief = new string('b', SurveyResponseFlag.MaxBriefLength);
        Assert.True((await edit.Handle(new UpdateResponseFlagBriefCommand(originalFlag.Id, fullBrief), default)).IsSuccess);
        var updated = (await queries.Handle(new GetResponseFlagQuery(originalFlag.Id), default)).Value;
        Assert.Equal(fullBrief, updated.Brief);
        Assert.Equal(fullAnswer, updated.Answer);
        Assert.Equal(SurveyResponseFlagType.Complaint, updated.Type);
        Assert.Equal(officer.ApplicationUserId, updated.BriefUpdatedBy!.ApplicationUserId);
        Assert.Equal(snapshot.Payload, (await db.Set<ResponseSnapshot>().AsNoTracking().SingleAsync(x => x.SurveyResponseId == responseId)).Payload);

        var reader = new SurveyDashboardComplaintReadService(store.Read<SurveyAnswer>(), store.Read<AnonymousSurveyAnswer>(),
            store.Read<SurveyResponse>(), store.Read<AnonymousSurveyResponse>());
        var filters = new SurveyDashboardComplaintReadRequest(true, true, firstBranch.Id, null, null,
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), 1, 10);
        var complaints = await reader.ReadAsync(filters, default);
        Assert.Equal(5, complaints.TotalComplaints);
        Assert.Equal(3, complaints.ResponsesWithComplaints);
        Assert.Equal(3, complaints.TotalResponses);
        var complaintGroups = complaints.TemplateGroups.ToDictionary(x => x.TemplateId);
        Assert.Equal(2, complaintGroups.Count);
        Assert.Equal((4, 2), (complaintGroups[first.Id].TotalComplaints, complaintGroups[first.Id].ResponsesWithComplaints));
        Assert.Equal((1, 1), (complaintGroups[anonymous.Id].TotalComplaints, complaintGroups[anonymous.Id].ResponsesWithComplaints));

        var items = complaints.PageItems.Select(x => GetSurveyDashboardComplaintsQueryHandler.MapItem(x, false)).ToArray();
        var questionItems = items.Where(x => x.ItemKind == "QuestionAnswer").ToArray();
        Assert.Equal(3, questionItems.Length);
        Assert.All(questionItems, x =>
        {
            Assert.Equal(question.Id, x.QuestionId);
            Assert.Null(x.FlagId);
            Assert.Equal(x.Source == SurveyDashboardSource.Anonymous ? anonymousComplaint : authorizedComplaint, x.ComplaintText);
        });
        Assert.Single(questionItems, x => x.Source == SurveyDashboardSource.Anonymous);

        var flagItems = items.Where(x => x.ItemKind == "ResponseFlag").ToArray();
        Assert.Equal(2, flagItems.Length);
        Assert.All(flagItems, x =>
        {
            Assert.Equal(SurveyDashboardSource.Internal, x.Source);
            Assert.Null(x.QuestionId);
            Assert.NotNull(x.FlagId);
        });
        var answeredFlag = Assert.Single(flagItems, x => x.FlagId == originalFlag.Id);
        Assert.Equal(fullBrief, answeredFlag.Brief);
        Assert.Equal(fullBrief, answeredFlag.ComplaintText);
        Assert.Equal(fullAnswer, answeredFlag.Answer);
        Assert.Single(flagItems, x => x.Brief is null && x.Answer is null);

        var expectedPageOrder = complaints.PageItems.Select(x => x.ComplaintId).ToArray();
        for (var pageNumber = 1; pageNumber <= expectedPageOrder.Length; pageNumber++)
        {
            var result = await reader.ReadAsync(filters with { PageNumber = pageNumber, PageSize = 1 }, default);
            Assert.Equal(expectedPageOrder.Length, result.TotalComplaints);
            Assert.Equal(expectedPageOrder[pageNumber - 1], Assert.Single(result.PageItems).ComplaintId);
        }
        var otherBranch = await reader.ReadAsync(filters with { BranchId = secondBranch.Id }, default);
        Assert.Equal(crossBranch.Id, Assert.Single(otherBranch.PageItems).ResponseId);
        var internalOnly = await reader.ReadAsync(filters with { IncludeAnonymous = false, InternalTemplateId = first.Id }, default);
        Assert.Equal(4, internalOnly.TotalComplaints);
        Assert.All(internalOnly.PageItems, x => Assert.Equal(first.Id, x.TemplateId));
        var anonymousOnly = await reader.ReadAsync(filters with { IncludeInternal = false, AnonymousTemplateId = anonymous.Id }, default);
        Assert.Equal(anonymousResponse.Id, Assert.Single(anonymousOnly.PageItems).ResponseId);
        var otherTemplate = await reader.ReadAsync(filters with { IncludeAnonymous = false, InternalTemplateId = second.Id }, default);
        Assert.Equal(0, otherTemplate.TotalComplaints);
        var outsideDate = await reader.ReadAsync(filters with
        {
            FromUtc = filters.ToExclusiveUtc,
            ToExclusiveUtc = filters.ToExclusiveUtc.AddDays(1)
        }, default);
        Assert.Equal(0, outsideDate.TotalComplaints);
        var suggestions = await reader.ReadAsync(filters with { FlagType = SurveyResponseFlagType.Suggestion }, default);
        Assert.Equal(2, suggestions.TotalComplaints);
        Assert.Equal(1, suggestions.ResponsesWithComplaints);
        Assert.Single(suggestions.PageItems, x => x.Brief is null);
        var comments = await reader.ReadAsync(filters with { FlagType = SurveyResponseFlagType.Comment }, default);
        Assert.Equal(2, comments.TotalComplaints);

        var dashboardSpec = new GetSurveyDashboardInternalResponsesSpec(firstBranch.Id, filters.FromUtc, filters.ToExclusiveUtc, null);
        var dashboardRows = await store.Read<SurveyResponse>().ListAsync(dashboardSpec);
        Assert.Equal(2, dashboardRows.Sum(x => x.ComplaintFlagsCount));
        Assert.Equal(2, dashboardRows.Sum(x => x.SuggestionsCount));
        Assert.Equal(2, dashboardRows.Sum(x => x.CommentsCount));

        var scopedSpec = new GetDepartmentSurveyResponsesPaginationSpec(store.Department.Id, filters.FromUtc, filters.ToExclusiveUtc,
            new GetDepartmentSurveyResponsesPaginationQuery { TemplateId = first.Id, HasComplaint = true });
        var scoped = new AuthorizedFeedbackItems(store.Read<SurveyResponse>(), store.Read<SurveyResponseFlag>());
        var paging = new BuildingBlock.Domain.SharedDto.SearchParameters { PageSize = 1 };
        var scopedItems = new List<DepartmentSurveyResponsePaginationItemResponse>();
        for (var pageNumber = 1; pageNumber <= 4; pageNumber++)
        {
            paging.PageNumber = pageNumber;
            var page = await scoped.PageAsync(scopedSpec, paging, SurveyResponseFlagType.Complaint,
                null, default, row => row.SurveyResponseId, (row, item, flag) => row with
                {
                    FlagId = item.FlagId, Flag = flag, ComplaintQuestionId = item.QuestionId, ComplaintText = item.Text
                });
            Assert.Equal(4, page.TotalItems);
            scopedItems.Add(Assert.Single(page.Data));
        }

        Assert.Equal(4, scopedItems.Select(x => (x.SurveyResponseId, x.FlagId, x.ComplaintQuestionId)).Distinct().Count());
        Assert.Equal(2, scopedItems.Count(x => x.FlagId.HasValue));
        Assert.All(scopedItems.Where(x => x.ComplaintQuestionId.HasValue), x => Assert.Equal(authorizedComplaint, x.ComplaintText));
        var scopedFlag = Assert.Single(scopedItems, x => x.FlagId == originalFlag.Id);
        Assert.Equal(fullBrief, scopedFlag.ComplaintText);
        Assert.Equal(fullAnswer, scopedFlag.Flag?.Answer);
        Assert.Equal(2, scopedItems.Count(x => x.ComplaintQuestionId == question.Id));
        Assert.Single(scopedItems, x => x.FlagId.HasValue && x.ComplaintText is null);

        var flagOnly = new GetSystemSurveyResponsesPaginationSpec(filters.FromUtc, filters.ToExclusiveUtc,
            new GetSystemSurveyResponsesPaginationQuery { BranchId = secondBranch.Id, HasComplaint = true });
        Assert.Equal(crossBranch.Id, Assert.Single(await store.Read<SurveyResponse>().ListAsync(flagOnly)).SurveyResponseId);
        var noComplaint = new GetSystemSurveyResponsesPaginationSpec(filters.FromUtc, filters.ToExclusiveUtc,
            new GetSystemSurveyResponsesPaginationQuery { BranchId = secondBranch.Id, HasComplaint = false });
        Assert.Equal(unflagged.Id, Assert.Single(await store.Read<SurveyResponse>().ListAsync(noComplaint)).SurveyResponseId);
        await VerifyDashboardsAsync(store, firstBranch.Id, first.Id);
        await VerifySatisfactionBreakdownAsync(store, op, first, group.Id);
        await VerifyFailedMediaSubmissionAsync(store, op, second);
    }

    private static async Task VerifySatisfactionBreakdownAsync(
        FlagStore store, Operator op, Template unscoredTemplate, Guid questionGroupId)
    {
        var actor = store.Admin.Id;
        var branchId = unscoredTemplate.BranchId;
        var template = Template.Create(branchId, "Scored", null, null, DateTime.UtcNow.AddDays(-1), null, actor);
        var rating = Question.Create(branchId, questionGroupId, "Rating", null, QuestionType.StarRating, actor);
        store.Db.AddRange(template, rating, TemplateQuestion.Create(template.Id, rating.Id, 1, actor));
        await store.Db.SaveChangesAsync();

        foreach (var score in new[] { 1, 3, 5 })
        {
            var response = SurveyResponse.Create(op.Id, template.Id, op.ApplicationUserId, score, 5, score * 20m);
            response.AddAnswer(SurveyAnswer.CreateStarRating(response.Id, rating.Id, score));
            if (score == 1)
            {
                response.AddSubmittedFlag(SurveyResponseFlagType.Complaint, null);
                response.AddSubmittedFlag(SurveyResponseFlagType.Complaint, null);
                response.AddSubmittedFlag(SurveyResponseFlagType.Suggestion, null);
            }

            store.Db.Add(response);
        }

        await store.Db.SaveChangesAsync();
        var handler = new GetBranchSatisfactionReportQueryHandler(store.Read<Template>(), store.Read<SurveyResponse>(),
            store.Read<SurveyAnswer>(), new TestBranchScope(branchId, actor), new TestCurrentUser(actor));
        var report = (await handler.Handle(new GetBranchSatisfactionReportQuery(), default)).Value;

        var scored = Assert.Single(report.ByTemplate);
        Assert.Equal(template.Id, scored.TemplateId);
        Assert.Equal((60m, 3, 3), (scored.Score, scored.ResponsesCount, scored.ScoredAnswersCount));
        Assert.Equal((2, 1, 0), (scored.ComplaintsCount, scored.SuggestionsCount, scored.CommentsCount));

        var feedback = report.FeedbackByTemplate.ToDictionary(x => x.TemplateId);
        Assert.Equal(2, feedback.Count);
        var unscoredFeedback = feedback[unscoredTemplate.Id];
        Assert.Equal((2, 4, 2, 2), (unscoredFeedback.ResponsesCount, unscoredFeedback.ComplaintsCount,
            unscoredFeedback.SuggestionsCount, unscoredFeedback.CommentsCount));
        var scoredFeedback = feedback[template.Id];
        Assert.Equal((3, 2, 1, 0), (scoredFeedback.ResponsesCount, scoredFeedback.ComplaintsCount,
            scoredFeedback.SuggestionsCount, scoredFeedback.CommentsCount));

        Assert.Equal(60m, report.Overall.SatisfactionPercentage);
        Assert.Equal(report.Overall.TotalResponses, feedback.Values.Sum(x => x.ResponsesCount));
        Assert.Equal(report.Overall.ComplaintsCount, feedback.Values.Sum(x => x.ComplaintsCount));
        Assert.Equal(report.Overall.SuggestionsCount, feedback.Values.Sum(x => x.SuggestionsCount));
        Assert.Equal(report.Overall.CommentsCount, feedback.Values.Sum(x => x.CommentsCount));
        Assert.Equal(3, report.Complaints.ResponsesWithComplaints);
    }

    private static async Task VerifyFailedMediaSubmissionAsync(FlagStore store, Operator op, Template template)
    {
        var db = store.Db;
        var actor = store.Admin.Id;
        var group = QuestionGroup.Create(template.BranchId, "Voice feedback", null, actor);
        var voice = Question.Create(template.BranchId, group.Id, "Voice recording", null, QuestionType.Voice, actor);
        db.AddRange(group, voice, TemplateQuestion.Create(template.Id, voice.Id, 1, actor),
            OperatorTemplate.Create(op.Id, template.Id, actor));
        await db.SaveChangesAsync();

        var responseCount = await db.Set<SurveyResponse>().CountAsync();
        var flagCount = await db.Set<SurveyResponseFlag>().CountAsync();
        // The media provider returns a filename that fails the SQL column limit after upload.
        var invalidFileName = new string('x', 301) + ".mp3";
        var media = new SubmissionMedia(invalidFileName);
        var submission = new SubmitOperatorTemplateResponseCommandHandler(store.Read<Operator>(),
            store.Read<OperatorTemplate>(), store.Read<TemplateQuestion>(), store.Read<TemplateCustomInput>(),
            store.Read<QuestionOption>(), store.Read<TemplateQuestionCondition>(), store.Write<SurveyResponse>(),
            media, new TestCurrentUser(op.ApplicationUserId), store.Unit, store.Permissions);
        await using var contents = new MemoryStream([1]);
        var request = new SubmitOperatorTemplateResponseCommand
        {
            TemplateId = template.Id,
            Answers = [new SubmitOperatorTemplateAnswerCommandItem
            {
                QuestionId = voice.Id,
                VoiceFile = new FormFile(contents, 0, contents.Length, "VoiceFile", "feedback.mp3")
            }],
            Flags = [new SubmitSurveyFlagCommandItem(SurveyResponseFlagType.Complaint, null)]
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => submission.Handle(request, default));
        Assert.Equal(invalidFileName, Path.GetFileName(Assert.Single(media.RemovedFiles)));
        db.ChangeTracker.Clear();
        Assert.Equal(responseCount, await db.Set<SurveyResponse>().CountAsync());
        Assert.Equal(flagCount, await db.Set<SurveyResponseFlag>().CountAsync());
    }

    private static async Task VerifyDashboardsAsync(FlagStore store, Guid branchId, Guid templateId)
    {
        var actor = store.Admin.Id;
        var current = new TestCurrentUser(actor);
        var scope = new TestBranchScope(branchId, actor);
        var responseRepo = store.Read<SurveyResponse>();
        var answerRepo = store.Read<SurveyAnswer>();
        var inputs = store.Read<SurveyResponseCustomInputValue>();
        var templateRepo = store.Read<Template>();

        var branch = new GetBranchDashboardQueryHandler(store.Read<Branch>(), templateRepo, responseRepo, answerRepo, inputs, scope, current);
        var branchSummary = (await branch.Handle(new GetBranchDashboardQuery { TemplateId = templateId }, default)).Value.Summary;
        Assert.Equal((4, 2, 2), (branchSummary.ComplaintsCount, branchSummary.SuggestionsCount, branchSummary.CommentsCount));

        var satisfaction = new GetBranchSatisfactionReportQueryHandler(templateRepo, responseRepo, answerRepo, scope, current);
        var satisfactionReport = (await satisfaction.Handle(new GetBranchSatisfactionReportQuery { TemplateId = templateId }, default)).Value;
        Assert.Equal((4, 2, 2), (satisfactionReport.Overall.ComplaintsCount, satisfactionReport.Overall.SuggestionsCount, satisfactionReport.Overall.CommentsCount));
        Assert.Equal(2, satisfactionReport.Complaints.ResponsesWithComplaints);

        var system = new GetSystemDashboardQueryHandler(store.Read<SuperAdmin>(), store.Read<Branch>(), store.Read<Department>(),
            store.Read<Operator>(), templateRepo, responseRepo, answerRepo, inputs, current);
        var systemSummary = (await system.Handle(new GetSystemDashboardQuery { BranchId = branchId }, default)).Value.Summary;
        Assert.Equal((4, 2, 2), (systemSummary.ComplaintsCount, systemSummary.SuggestionsCount, systemSummary.CommentsCount));

        var admin = NewUser(UserType.DepartmentAdmin, actor);
        store.Db.AddRange(admin, DepartmentAdmin.Create(admin.Id, store.Department.Id, actor));
        await store.Db.SaveChangesAsync();
        var department = new GetDepartmentDashboardQueryHandler(store.Read<DepartmentAdmin>(), store.Read<Operator>(),
            store.Read<OperatorTemplate>(), templateRepo, responseRepo, answerRepo, inputs, new TestCurrentUser(admin.Id));
        var departmentSummary = (await department.Handle(new GetDepartmentDashboardQuery { TemplateId = templateId }, default)).Value.Summary;
        Assert.Equal((4, 2, 2), (departmentSummary.ComplaintsCount, departmentSummary.SuggestionsCount, departmentSummary.CommentsCount));

        var survey = new GetSurveyDashboardQueryHandler(store.Read<SuperAdmin>(), store.Read<Branch>(), templateRepo,
            store.Read<AnonymousTemplate>(), responseRepo, answerRepo, inputs, store.Read<AnonymousSurveyResponse>(),
            store.Read<AnonymousSurveyAnswer>(), store.Read<AnonymousSurveyResponseCustomInputValue>(),
            store.Read<TemplateQuestion>(), store.Read<AnonymousTemplateQuestion>(), store.Read<TemplateQuestionCondition>(),
            store.Read<AnonymousTemplateQuestionCondition>(), store.Read<QuestionOption>(), new SurveyReportScoringService(), scope, current);
        var surveySummary = (await survey.Handle(new GetSurveyDashboardQuery { BranchId = branchId }, default)).Value.Summary;
        Assert.Equal((5, 2, 2), (surveySummary.ComplaintsCount, surveySummary.SuggestionsCount, surveySummary.CommentsCount));
    }

    private sealed class TestBranchScope(Guid branchId, Guid actor) : ICurrentBranchScopeResolver
    {
        public Task<Result<CurrentBranchScope>> ResolveAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result<CurrentBranchScope>.Ok(new CurrentBranchScope
            {
                ApplicationUserId = actor, BranchId = branchId, ActorType = UserType.BranchAdmin
            }));
    }

    [SqlHistoryFact]
    public async Task OfficerLifecycleAndPasswordResetRespectDepartmentAndRoles()
    {
        await using var store = await FlagStore.CreateAsync();
        var actor = store.Admin.Id;
        var departmentUser = NewUser(UserType.DepartmentAdmin, actor);
        store.Db.AddRange(departmentUser, DepartmentAdmin.Create(departmentUser.Id, store.Department.Id, actor));
        await store.Db.SaveChangesAsync();

        var created = (await store.CreateOfficer(departmentUser.Id, null)).Value;
        Assert.Equal(store.Department.Id, created.DepartmentId);
        Assert.True((await store.CreateOfficer(departmentUser.Id, store.OtherDepartment.Id)).IsFailure);
        var outside = (await store.CreateOfficer(actor, store.OtherDepartment.Id)).Value;

        var roleHandler = store.AssignRoles(departmentUser.Id);
        Assert.True((await roleHandler.Handle(new AssignFlagOfficerRolesCommand(outside.FlagOfficerId, []), default)).IsFailure);
        var systemRole = await store.Db.Set<Role>().SingleAsync(x => x.Name == SeedConstants.RoleNames.SystemAdministrator);
        Assert.True((await roleHandler.Handle(new AssignFlagOfficerRolesCommand(created.FlagOfficerId, [systemRole.Id]), default)).IsFailure);

        var roles = await store.Read<RolePermission>().ListAsync(new FlagOfficerCapabilityRolesSpec());
        Assert.Equal(4, roles.Count);
        var view = roles.Single(x => x.Permission == FlagAccess.ResponseFlagsView);
        Assert.Single((await roleHandler.Handle(new AssignFlagOfficerRolesCommand(created.FlagOfficerId, [view.RoleId]), default)).Value);
        var officeQuery = new FlagOfficerQueryHandlers(store.Scope(departmentUser.Id), store.Read<FlagOfficer>(), store.Read<RolePermission>());
        Assert.True((await officeQuery.Handle(new GetFlagOfficerQuery(outside.FlagOfficerId), default)).IsFailure);
        var local = Assert.Single((await officeQuery.Handle(new GetFlagOfficersQuery(), default)).Value.Data);
        Assert.Equal(view.RoleId, Assert.Single(local.Roles).RoleId);

        var jwt = new CapturingJwt();
        var login = new LoginCommandHandler(store.Read<ApplicationUser>(), store.Read<UserRole>(), store.Read<RolePermission>(),
            store.Read<SuperAdmin>(), store.Read<BranchAdmin>(), store.Read<BranchUser>(), store.Read<BranchArea>(),
            store.Read<BranchAreaBranch>(), store.Read<DepartmentAdmin>(), store.Read<Operator>(), store.Read<FlagOfficer>(),
            jwt, Options.Create(new PasswordPolicyOptions()));
        var credentials = new LoginCommand { UserNameOrEmail = local.UserName, Password = Password };
        Assert.True((await login.Handle(credentials, default)).IsSuccess);
        Assert.Equal(FlagAccess.ResponseFlagsView, Assert.Single(jwt.Permissions));
        var orphan = NewUser(UserType.FlagOfficer, actor);
        store.Db.Add(orphan);
        await store.Db.SaveChangesAsync();
        Assert.Equal("Auth.Login.ActorProfileNotFound", Assert.Single((await login.Handle(
            credentials with { UserNameOrEmail = orphan.UserName }, default)).Errors).Code);

        var status = new ChangeFlagOfficerHandler(store.Scope(departmentUser.Id), store.Read<FlagOfficer>(),
            store.Read<ApplicationUser>(), store.Unit);
        Assert.True((await status.Handle(new DeactivateFlagOfficerCommand(created.FlagOfficerId), default)).IsSuccess);
        Assert.True((await login.Handle(credentials, default)).IsFailure);
        Assert.True((await status.Handle(new RestoreFlagOfficerCommand(created.FlagOfficerId), default)).IsSuccess);

        var reset = new ResetUserPasswordCommandHandler(store.Read<ApplicationUser>(), store.Write<ApplicationUser>(),
            store.Read<SuperAdmin>(), store.Read<BranchAdmin>(), store.Read<BranchArea>(), store.Read<DepartmentAdmin>(),
            store.Read<BranchUser>(), store.Read<Operator>(), store.Read<FlagOfficer>(), new TestCurrentUser(departmentUser.Id), store.Unit);
        var resetRequest = new ResetUserPasswordCommand
        {
            ApplicationUserId = outside.ApplicationUserId, NewPassword = "ChangedPassword123", ConfirmNewPassword = "ChangedPassword123"
        };
        Assert.Equal("Users.ResetPassword.ScopeViolation", Assert.Single((await reset.Handle(resetRequest, default)).Errors).Code);
        Assert.True((await reset.Handle(resetRequest with { ApplicationUserId = created.ApplicationUserId }, default)).IsSuccess);
        Assert.True((await login.Handle(credentials with { Password = resetRequest.NewPassword }, default)).IsSuccess);
    }

    private static ApplicationUser NewUser(UserType type, Guid actor)
    {
        var login = Guid.NewGuid().ToString("N");
        return ApplicationUser.Create(login, login + "@example.test", type.ToString(), null, null,
            new PasswordHasher<ApplicationUser>().HashPassword(null!, Password), type, actor);
    }

    private sealed class FlagStore(PlatformWriteDbContext db, ApplicationUser admin, Department department, Department otherDepartment)
        : IAsyncDisposable
    {
        public PlatformWriteDbContext Db => db;
        public ApplicationUser Admin => admin;
        public Department Department => department;
        public Department OtherDepartment => otherDepartment;
        public IUnitOfWork Unit { get; } = new PlatformUnitOfWork(new PlatformWriteDbContextResolver(db));
        public FlagPermissions Permissions { get; } = new(new PlatformWriteReadRepository<UserRole>(new PlatformWriteReadDbContextResolver(db)));

        public IWriteReadRepository<T> Read<T>() where T : class => new PlatformWriteReadRepository<T>(new PlatformWriteReadDbContextResolver(db));
        public IWriteRepository<T> Write<T>() where T : class => new PlatformWriteRepository<T>(new PlatformWriteDbContextResolver(db));

        public FlagOfficerAdministrationScopeResolver Scope(Guid actor) => new(new TestCurrentUser(actor), Read<SuperAdmin>(), Read<DepartmentAdmin>());
        public FlagOfficerAccess Access(Guid userId) => new(new TestCurrentUser(userId), Read<FlagOfficer>(), Permissions);
        public ResponseFlagQueryHandlers Queries(FlagOfficerAccess access) => new(access, Read<SurveyResponse>(), Read<SurveyResponseFlag>(),
            new AuthorizedResponseDetailsReader(Read<SurveyResponse>(), Read<SurveyAnswer>(), Read<SurveyResponseCustomInputValue>(), Read<QuestionOption>()));

        public Task<BuildingBlock.Domain.Results.Result<FlagOfficerChange>> CreateOfficer(Guid actor, Guid? departmentId)
        {
            var handler = new CreateFlagOfficerHandler(Scope(actor), Read<Department>(), Read<ApplicationUser>(), Read<Role>(),
                Write<ApplicationUser>(), Write<FlagOfficer>(), Write<UserRole>(), Unit);
            var login = Guid.NewGuid().ToString("N");
            return handler.Handle(new CreateFlagOfficerCommand(departmentId, login, Password,
                new FlagOfficerProfile("Flag Officer", null, login + "@example.test", null)), default);
        }

        public AssignFlagOfficerRolesHandler AssignRoles(Guid actor)
            => new(Scope(actor), Read<FlagOfficer>(), Read<RolePermission>(), Read<UserRole>(), Write<UserRole>(), Unit);

        public async Task Grant(FlagOfficerChange officer, params string[] roles)
        {
            var available = await Read<RolePermission>().ListAsync(new FlagOfficerCapabilityRolesSpec());
            var selected = available.Where(x => roles.Contains(x.Name)).Select(x => x.RoleId).ToArray();
            Assert.Equal(roles.Length, selected.Length);
            Assert.True((await AssignRoles(Admin.Id).Handle(new AssignFlagOfficerRolesCommand(officer.FlagOfficerId, selected), default)).IsSuccess);
        }

        public static async Task<FlagStore> CreateAsync()
        {
            var database = "CustomerSurveyFlagsTests_" + Guid.NewGuid().ToString("N");
            var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
                .UseSqlServer($@"Server=(localdb)\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true")
                .Options;
            var db = new PlatformWriteDbContext(options, new Tenant());
            try
            {
                await db.Database.MigrateAsync();
                var admin = NewUser(UserType.SuperAdmin, Guid.NewGuid());
                var home = Department.Create("Home", null, admin.Id);
                var other = Department.Create("Other", null, admin.Id);
                db.AddRange(admin, SuperAdmin.Create(admin.Id, admin.Id), home, other);
                db.AddRange(SeedConstants.SeedCatalog.Roles.Select(x => Role.CreateSeeded(x.Id, x.Name, admin.Id)));
                db.AddRange(SeedConstants.SeedCatalog.Permissions.Select(x => Permission.CreateSeeded(x.Id, x.Name, admin.Id)));
                await db.SaveChangesAsync();
                db.AddRange(SeedConstants.SeedCatalog.RolePermissions.Select(x => RolePermission.Create(x.RoleId, x.PermissionId, admin.Id)));
                await db.SaveChangesAsync();
                return new FlagStore(db, admin, home, other);
            }
            catch
            {
                await db.Database.EnsureDeletedAsync();
                await db.DisposeAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await db.Database.EnsureDeletedAsync();
            await db.DisposeAsync();
        }
    }

    private sealed class CapturingJwt : IJwtProvider
    {
        public IReadOnlyCollection<string> Permissions { get; private set; } = [];
        public Task<UserTokenDto> Generate(Guid userId, string email, string phoneNumber,
            IReadOnlyCollection<string> roleNames, UserType userType, IReadOnlyCollection<string> permissions,
            Guid? activeBranchId = null, CancellationToken cancellationToken = default)
        {
            Permissions = permissions;
            return Task.FromResult(new UserTokenDto { UserType = userType.ToString() });
        }
    }

    private sealed class Tenant : ICurrentTenantContext
    {
        public TenantMode Mode => default;
        public Guid? AccountId => null;
        public Guid? UserId => null;
        public bool IsAuthenticated => true;
        public string? Role => null;
        public BuildingBlock.Application.Abstraction.Security.UserType UserType => default;
        public bool HasAccount => false;
        public bool IsPlatformAdmin => true;
    }

    private sealed class SubmissionMedia(string? fileName = null) : IMediaService
    {
        public List<string> RemovedFiles { get; } = [];

        public Task<string> SaveAsync(IFormFile mediaFile, string folderName)
            => fileName is null ? throw new NotSupportedException() : Task.FromResult(fileName);
        public Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName) => throw new NotSupportedException();
        public Task<string> CopyAsync(string relativePath, string folderName) => throw new NotSupportedException();
        public Task<Stream> GetStream(IFormFile formFile) => throw new NotSupportedException();
        public void Remove(string filePath) => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<string> filePaths) => RemovedFiles.AddRange(filePaths);
        public Task<string> SaveVideoAsync(IFormFile mediaFile, string folderName) => throw new NotSupportedException();
    }
}
