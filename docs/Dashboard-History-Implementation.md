# CustomerSurvey Dashboard, Reporting, Scoring and Historical Data — Implementation Report

## A. Overall Status

Completed in `codex/dashboard-reporting-history`, based on local master `9051850`.

All ten approved backend decisions are implemented: three scoring modes; immutable original scores; immutable response snapshots and bounded legacy reconstruction; NoData; consistent mode selection; equal response weighting; template availability history; exact TemplateId selection; current branch classification settings; and BestRatedQuestions.

Historical information that never existed cannot be recovered. Such data is explicitly incomplete, alternate-mode scores use an identified stored root baseline, and uncertain historical question/group tokens are excluded. Production migrations and maintenance remain deployment operations. No frontend was requested.

## B. Implementation Summary

1. **Critical Reporting Fixes:** Removed TemplateFamily expansion, preserved source/branch ownership validation and Excel's one-template check, and made complaint counting independent of detailed response rendering. Complaint item and response counts remain distinct.
2. **Database & Snapshots:** Added settings, snapshot and availability-event persistence. The existing write context captures new Operator/Anonymous snapshots during the submission Unit of Work. Snapshot creation failure fails the save and follows the existing media compensation path. Snapshots preserve historical question/group labels, types, ordering, root/conditional position, selected values, all available options, condition paths, custom-input definitions and media references. Existing custom-input value rows are reused. Detail reads are snapshot-aware; ordinary pagination does not expose storage payloads.
3. **Scoring & Classification:** Added AllAnsweredQuestions=3; preserved RootQuestions=1 and LowestConditionLevel=2. Numeric scales use five. Overall and group metrics weight responses equally. Immutable score/membership columns allow SQL filtering, counting and paging in the selected mode. Current originating-branch settings drive categories and risk; mixed aggregate criteria are weighted by scored-response contributions.
4. **Template Availability History:** The write context records actual availability changes for all legitimate mutation paths, including copies/assignments, in the same save. Unrelated name/logo edits do not generate availability transitions. Historical activation, scheduling and report intervals are intersected; natural expiry uses ExpireTo. Legacy baselines begin known coverage at initialization, without inventing earlier creation or activation.
5. **Dashboards & Reports:** Unified, Branch, Department, Anonymous and System calculations use selected scores. Applicable dashboards include BestRatedQuestions, NoData flags, nullable satisfaction alternatives, historical availability and scoring completeness. Rankings combine historical label versions by question identity, keep deterministic ties and preserve mode/scope in navigation. Complaint-item paging reuses existing routes and permissions. JSON/PDF/Excel share the report scoring source.
6. **Tests:** Added historical scoring/settings, availability, SQL persistence and controller contract coverage; updated family-selection tests and PDF mode rendering tests. Existing submission, custom-input, media-validation and copy tests were included in the focused regression run.

## C. Changed Files

The complete manifest follows, grouped by layer. Generated migration designers and resource designer output are omitted from this list; their corresponding migrations/resources are included.

**Domain**

- [CustomerSurvey.Domain/Entities/AnonymousSurveyResponse.cs](<E:/Customer Servey/CustomerSurvey.Domain/Entities/AnonymousSurveyResponse.cs>)
- [CustomerSurvey.Domain/Entities/Branch.cs](<E:/Customer Servey/CustomerSurvey.Domain/Entities/Branch.cs>)
- [CustomerSurvey.Domain/Entities/BranchEvaluationSettings.cs](<E:/Customer Servey/CustomerSurvey.Domain/Entities/BranchEvaluationSettings.cs>)
- [CustomerSurvey.Domain/Entities/ResponseSnapshot.cs](<E:/Customer Servey/CustomerSurvey.Domain/Entities/ResponseSnapshot.cs>)
- [CustomerSurvey.Domain/Entities/SnapshotScoreSelection.cs](<E:/Customer Servey/CustomerSurvey.Domain/Entities/SnapshotScoreSelection.cs>)
- [CustomerSurvey.Domain/Entities/SurveyResponse.cs](<E:/Customer Servey/CustomerSurvey.Domain/Entities/SurveyResponse.cs>)
- [CustomerSurvey.Domain/Entities/TemplateAvailabilityEvent.cs](<E:/Customer Servey/CustomerSurvey.Domain/Entities/TemplateAvailabilityEvent.cs>)
- [CustomerSurvey.Domain/Resources/ErrorMessage.ar.resx](<E:/Customer Servey/CustomerSurvey.Domain/Resources/ErrorMessage.ar.resx>)
- [CustomerSurvey.Domain/Resources/ErrorMessage.resx](<E:/Customer Servey/CustomerSurvey.Domain/Resources/ErrorMessage.resx>)

**Application**

- [CustomerSurvey.Application/Abstraction/Reports/ISurveyDashboardComplaintReadService.cs](<E:/Customer Servey/CustomerSurvey.Application/Abstraction/Reports/ISurveyDashboardComplaintReadService.cs>)
- [CustomerSurvey.Application/Abstraction/Reports/ITemplateAvailabilityReader.cs](<E:/Customer Servey/CustomerSurvey.Application/Abstraction/Reports/ITemplateAvailabilityReader.cs>)
- [CustomerSurvey.Application/Features/AnonTemplates/Command/SubmitAnonymousTemplateResponse/SubmitAnonymousTemplateResponseCommandHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonTemplates/Command/SubmitAnonymousTemplateResponse/SubmitAnonymousTemplateResponseCommandHandler.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/AnonymousDashboardSurveyAnswerDto.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/AnonymousDashboardSurveyAnswerDto.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/AnonymousDashboardSurveyResponseDto.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/AnonymousDashboardSurveyResponseDto.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousDashboardSurveyResponsesSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousDashboardSurveyResponsesSpec.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousTemplateDashboardQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousTemplateDashboardQuery.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousTemplateDashboardQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousTemplateDashboardQueryHandler.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousTemplateDashboardQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousTemplateDashboardQueryValidator.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousTemplateDashboardResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetAnonymousTemplateDashboardResponse.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetBranchAnonymousTemplatesForDashboardSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateDashboard/GetBranchAnonymousTemplatesForDashboardSpec.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponseDetails/GetAnonymousTemplateResponseDetailsQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponseDetails/GetAnonymousTemplateResponseDetailsQueryHandler.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponseDetails/GetAnonymousTemplateResponseDetailsResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponseDetails/GetAnonymousTemplateResponseDetailsResponse.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/AnonymousTemplateResponsePaginationItemResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/AnonymousTemplateResponsePaginationItemResponse.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/GetAnonymousTemplateResponsesPaginationQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/GetAnonymousTemplateResponsesPaginationQuery.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/GetAnonymousTemplateResponsesPaginationQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/GetAnonymousTemplateResponsesPaginationQueryHandler.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/GetAnonymousTemplateResponsesPaginationQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/GetAnonymousTemplateResponsesPaginationQueryValidator.cs>)
- [CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/GetAnonymousTemplateResponsesPaginationSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/AnonymousTemplates/Query/GetAnonymousTemplateResponsesPagination/GetAnonymousTemplateResponsesPaginationSpec.cs>)
- [CustomerSurvey.Application/Features/Branches/EvaluationSettings/BranchEvaluationSettingsFeature.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Branches/EvaluationSettings/BranchEvaluationSettingsFeature.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/BranchAnonymousResponsePaginationItemResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/BranchAnonymousResponsePaginationItemResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/GetBranchAnonymousResponsesPaginationQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/GetBranchAnonymousResponsesPaginationQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/GetBranchAnonymousResponsesPaginationQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/GetBranchAnonymousResponsesPaginationQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/GetBranchAnonymousResponsesPaginationQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/GetBranchAnonymousResponsesPaginationQueryValidator.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/GetBranchAnonymousResponsesPaginationSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchAnonymousResponsesPagination/GetBranchAnonymousResponsesPaginationSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/DashboardSurveyAnswerDto.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/DashboardSurveyAnswerDto.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/DashboardSurveyResponseDto.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/DashboardSurveyResponseDto.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardQueryValidator.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchTemplatesForDashboardSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchTemplatesForDashboardSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetDashboardSurveyResponsesSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetDashboardSurveyResponsesSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportQueryValidator.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetSatisfactionSurveyResponsesForBranchReportSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetSatisfactionSurveyResponsesForBranchReportSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/SatisfactionSurveyResponseFlatDto.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/SatisfactionSurveyResponseFlatDto.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponseDetails/GetBranchSurveyResponseDetailsQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponseDetails/GetBranchSurveyResponseDetailsQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponseDetails/GetBranchSurveyResponseDetailsResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponseDetails/GetBranchSurveyResponseDetailsResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponsesPagination/BranchSurveyResponsePaginationItemResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponsesPagination/BranchSurveyResponsePaginationItemResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponsesPagination/GetBranchSurveyResponsesPaginationQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponsesPagination/GetBranchSurveyResponsesPaginationQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponsesPagination/GetBranchSurveyResponsesPaginationQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponsesPagination/GetBranchSurveyResponsesPaginationQueryValidator.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponsesPagination/GetBranchSurveyResponsesPaginationSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponsesPagination/GetBranchSurveyResponsesPaginationSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchTemplatesPdfReport/BranchTemplatesPdfReportModel.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchTemplatesPdfReport/BranchTemplatesPdfReportModel.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetBranchTemplatesPdfReport/ScoreCalculationMode.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetBranchTemplatesPdfReport/ScoreCalculationMode.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/DepartmentDashboardSurveyAnswerDto.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/DepartmentDashboardSurveyAnswerDto.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/DepartmentDashboardSurveyResponseDto.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/DepartmentDashboardSurveyResponseDto.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardAssignedTemplatesSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardAssignedTemplatesSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardQueryValidator.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardSurveyResponsesSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardSurveyResponsesSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponseDetails/GetDepartmentOperatorSurveyResponseDetailsQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponseDetails/GetDepartmentOperatorSurveyResponseDetailsQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponseDetails/GetDepartmentOperatorSurveyResponseDetailsResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponseDetails/GetDepartmentOperatorSurveyResponseDetailsResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponsesPagination/DepartmentOperatorSurveyResponsePaginationItemResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponsesPagination/DepartmentOperatorSurveyResponsePaginationItemResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponsesPagination/GetDepartmentOperatorSurveyResponsesPaginationQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponsesPagination/GetDepartmentOperatorSurveyResponsesPaginationQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponsesPagination/GetDepartmentOperatorSurveyResponsesPaginationQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponsesPagination/GetDepartmentOperatorSurveyResponsesPaginationQueryValidator.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponsesPagination/GetDepartmentOperatorSurveyResponsesPaginationSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponsesPagination/GetDepartmentOperatorSurveyResponsesPaginationSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/DepartmentSurveyResponsePaginationItemResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/DepartmentSurveyResponsePaginationItemResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationQueryValidator.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardAnonymousResponsesSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardAnonymousResponsesSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardInternalResponsesSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardInternalResponsesSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboardQuestionGroups/GetSurveyDashboardQuestionGroupsQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboardQuestionGroups/GetSurveyDashboardQuestionGroupsQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboardQuestionGroups/SurveyDashboardQuestionGroupsResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboardQuestionGroups/SurveyDashboardQuestionGroupsResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSurveyResponsesPagination/GetSurveyResponsesPaginationQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSurveyResponsesPagination/GetSurveyResponsesPaginationQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSurveyResponsesPagination/GetSurveyResponsesPaginationQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSurveyResponsesPagination/GetSurveyResponsesPaginationQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSurveyResponsesPagination/SurveyResponsePaginationItemResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSurveyResponsesPagination/SurveyResponsePaginationItemResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardQueryValidator.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardSurveyResponsesSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardSurveyResponsesSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/SystemDashboardSurveyResponseDto.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/SystemDashboardSurveyResponseDto.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponseDetails/GetSystemSurveyResponseDetailsQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponseDetails/GetSystemSurveyResponseDetailsQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponseDetails/GetSystemSurveyResponseDetailsResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponseDetails/GetSystemSurveyResponseDetailsResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationQuery.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationQuery.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationQueryHandler.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationQueryHandler.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationQueryValidator.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationQueryValidator.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationSpec.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationSpec.cs>)
- [CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/SystemSurveyResponsePaginationItemResponse.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/SystemSurveyResponsePaginationItemResponse.cs>)
- [CustomerSurvey.Application/Features/Reports/Services/Scoring/DashboardScoreSelection.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Services/Scoring/DashboardScoreSelection.cs>)
- [CustomerSurvey.Application/Features/Reports/Services/Scoring/ReportScoreAggregation.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Services/Scoring/ReportScoreAggregation.cs>)
- [CustomerSurvey.Application/Features/Reports/Services/Scoring/ReportScoringDtos.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Services/Scoring/ReportScoringDtos.cs>)
- [CustomerSurvey.Application/Features/Reports/Services/Scoring/SnapshotScoring.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Services/Scoring/SnapshotScoring.cs>)
- [CustomerSurvey.Application/Features/Reports/Services/Scoring/SurveyReportScoringService.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Services/Scoring/SurveyReportScoringService.cs>)
- [CustomerSurvey.Application/Features/Reports/Shared/ComplaintItemsSpecification.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Shared/ComplaintItemsSpecification.cs>)
- [CustomerSurvey.Application/Features/Reports/Shared/DashboardNavigation.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Shared/DashboardNavigation.cs>)
- [CustomerSurvey.Application/Features/Reports/Shared/DepartmentAvailabilityTemplateSpecs.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Shared/DepartmentAvailabilityTemplateSpecs.cs>)
- [CustomerSurvey.Application/Features/Reports/Shared/EvaluationCriteria.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Shared/EvaluationCriteria.cs>)
- [CustomerSurvey.Application/Features/Reports/Shared/ResponseSnapshotSpecs.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Shared/ResponseSnapshotSpecs.cs>)
- [CustomerSurvey.Application/Features/Reports/Shared/SatisfactionCategory.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Shared/SatisfactionCategory.cs>)
- [CustomerSurvey.Application/Features/Reports/Shared/ScoredResponseSpecification.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Shared/ScoredResponseSpecification.cs>)
- [CustomerSurvey.Application/Features/Reports/Shared/TemplateAvailabilityIntervals.cs](<E:/Customer Servey/CustomerSurvey.Application/Features/Reports/Shared/TemplateAvailabilityIntervals.cs>)

**Infrastructure**

- [CustomerSurvey.infrastructure/Bootstrap/Bootstrap.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Bootstrap/Bootstrap.cs>)
- [CustomerSurvey.infrastructure/Configurations/SurveyHistoryConfiguration.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Configurations/SurveyHistoryConfiguration.cs>)
- [CustomerSurvey.infrastructure/Migrations/20261021110352_SurveyHistoryAndBranchEvaluation.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Migrations/20261021110352_SurveyHistoryAndBranchEvaluation.cs>)
- [CustomerSurvey.infrastructure/Migrations/20261021113627_AddImmutableSnapshotScoreQueries.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Migrations/20261021113627_AddImmutableSnapshotScoreQueries.cs>)
- [CustomerSurvey.infrastructure/Migrations/PlatformWriteDbContextModelSnapshot.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Migrations/PlatformWriteDbContextModelSnapshot.cs>)
- [CustomerSurvey.infrastructure/Persistence/PlatformWriteDbContext.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Persistence/PlatformWriteDbContext.cs>)
- [CustomerSurvey.infrastructure/Persistence/ResponseSnapshotCapture.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Persistence/ResponseSnapshotCapture.cs>)
- [CustomerSurvey.infrastructure/Persistence/SurveyHistoryMaintenance.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Persistence/SurveyHistoryMaintenance.cs>)
- [CustomerSurvey.infrastructure/Reports/BranchTemplateExcelReportService.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Reports/BranchTemplateExcelReportService.cs>)
- [CustomerSurvey.infrastructure/Reports/BranchTemplatesPdfReportService.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Reports/BranchTemplatesPdfReportService.cs>)
- [CustomerSurvey.infrastructure/Reports/BranchTemplatesQuestionRankBuilder.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Reports/BranchTemplatesQuestionRankBuilder.cs>)
- [CustomerSurvey.infrastructure/Reports/SurveyDashboardComplaintReadService.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Reports/SurveyDashboardComplaintReadService.cs>)
- [CustomerSurvey.infrastructure/Reports/TemplateAvailabilityReader.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Reports/TemplateAvailabilityReader.cs>)
- [CustomerSurvey.infrastructure/Seeders/SeedConstants.cs](<E:/Customer Servey/CustomerSurvey.infrastructure/Seeders/SeedConstants.cs>)

**API**

- [CustomerSurvey.Api/Controllers/AnonymousTemplatesController.cs](<E:/Customer Servey/CustomerSurvey.Api/Controllers/AnonymousTemplatesController.cs>)
- [CustomerSurvey.Api/Controllers/BranchesController.cs](<E:/Customer Servey/CustomerSurvey.Api/Controllers/BranchesController.cs>)
- [CustomerSurvey.Api/Program.cs](<E:/Customer Servey/CustomerSurvey.Api/Program.cs>)
- [CustomerSurvey.Api/Views/Templates/BranchTemplatesPdfReport.cshtml](<E:/Customer Servey/CustomerSurvey.Api/Views/Templates/BranchTemplatesPdfReport.cshtml>)

**Tests**

- [CustomerSurvey.Tests/Reports/BranchTemplatesPdfRazorReportTests.cs](<E:/Customer Servey/CustomerSurvey.Tests/Reports/BranchTemplatesPdfRazorReportTests.cs>)
- [CustomerSurvey.Tests/Reports/HistoricalScoringAndSettingsTests.cs](<E:/Customer Servey/CustomerSurvey.Tests/Reports/HistoricalScoringAndSettingsTests.cs>)
- [CustomerSurvey.Tests/Reports/ReportingControllerContractTests.cs](<E:/Customer Servey/CustomerSurvey.Tests/Reports/ReportingControllerContractTests.cs>)
- [CustomerSurvey.Tests/Reports/SurveyHistorySqlTests.cs](<E:/Customer Servey/CustomerSurvey.Tests/Reports/SurveyHistorySqlTests.cs>)
- [CustomerSurvey.Tests/Reports/TemplateAvailabilityHistoryTests.cs](<E:/Customer Servey/CustomerSurvey.Tests/Reports/TemplateAvailabilityHistoryTests.cs>)
- [CustomerSurvey.Tests/Reports/TemplateFamilyReportResolutionTests.cs](<E:/Customer Servey/CustomerSurvey.Tests/Reports/TemplateFamilyReportResolutionTests.cs>)

Source-of-truth approved-rule addendum: [source of truth.txt](<E:/Customer Servey/source of truth.txt>).

## D. API Changes

Existing routes and submission request bodies are retained. Enum values in query strings can use their existing supported numeric/name representation; the API retains its existing JSON enum converter.

| HTTP Method | Endpoint | Added/Modified | Request Changes | Response Changes | Required Permission | Frontend Impact |
|---|---|---|---|---|---|---|
| GET | `/api/branches/{branchId}/evaluation-settings` | Added | Branch ID in path | Four effective thresholds, UsesDefaults, UpdatedOnUtc | `Branches.ViewEvaluationSettings` plus current branch scope | Read settings/defaults |
| PUT | `/api/branches/{branchId}/evaluation-settings` | Added | NeutralMinimum, SatisfiedMinimum, MediumRiskMinimum, HealthyMinimum; path identity authoritative | Effective settings and audit timestamp | `Branches.ManageEvaluationSettings` plus assigned BranchAdmin actor/profile | Settings form; refresh JWT after new permission seeding |
| GET | `/api/reports/survey-dashboard` | Modified | Existing mode accepts 3 | BestRatedQuestions, availability, HasScoreData/nullable SatisfactionPercentage, completeness/mixed criteria, scored-response navigation | `Reports.ViewBranchReports` plus existing scope | Mode selector, best rankings and N/A |
| GET | `/api/reports/branch-dashboard` | Modified | ScoreCalculationMode, default 1 | Same applicable additive dashboard fields | `Reports.ViewBranchReports` | Same |
| GET | `/api/reports/department-dashboard` | Modified | ScoreCalculationMode, default 1 | Same; deterministic mixed-branch policy | `Reports.ViewDepartmentReports` | Same |
| GET | `/api/anonymous-templates/dashboard` | Modified | ScoreCalculationMode, default 1 | Same applicable additive fields | `AnonymousTemplates.ViewResponses` | Same |
| GET | `/api/reports/system-dashboard` | Modified | ScoreCalculationMode, default 1 | Current branch classification, availability and scoring completeness; no unrelated Best-only feature | `Reports.ViewSystemDashboard` | Current criteria/N/A |
| GET | `/api/reports/branch/satisfaction` | Modified | ScoreCalculationMode, default 1 | Mode/completeness; HasScoreData and nullable SatisfactionPercentage | `Reports.ViewBranchReports` | Guard unavailable scores |
| GET | `/api/reports/survey-dashboard/question-groups` | Modified | Existing mode accepts 3 | Equal response weights, historical groups and completeness | `Reports.ViewBranchReports` | Display partial coverage |
| GET | `/api/reports/survey-dashboard/complaints` | Modified | Existing exact source/template/date filters | Historical question labels; complaint-answer counts retained | `Reports.ViewBranchReports` | Complaint KPI opens item list |
| GET | `/api/reports/survey-responses` | Modified | ScoreIncludedOnly for ranking navigation; existing mode accepts 3 | Selected scores, derivation/completeness | `Reports.ViewBranchReports` | Preserve mode and generated navigation |
| GET | `/api/reports/branch-responses` | Modified | ScoreCalculationMode, ScoreIncludedOnly | SQL-filtered selected scores, derivation | `Reports.ViewBranchReports` | Preserve mode |
| GET | `/api/reports/anonymous-responses` | Modified | ScoreCalculationMode, ScoreIncludedOnly, ComplaintItemsOnly | Selected scores; optional complaint-item paging | `Reports.ViewBranchReports` | Item key can include response/question identity |
| GET | `/api/reports/department-responses` | Modified | ScoreCalculationMode, ScoreIncludedOnly, ComplaintItemsOnly | Same, with existing department scope | `Reports.ViewDepartmentReports` | Complaint mode totals count answers |
| GET | `/api/reports/department-operators/{operatorId}/responses` | Modified | ScoreCalculationMode, ScoreIncludedOnly | Selected score filtering/paging and derivation | `Reports.ViewDepartmentReports` | Preserve operator/mode |
| GET | `/api/reports/system-responses` | Modified | ScoreCalculationMode, ScoreIncludedOnly, ComplaintItemsOnly | Selected score filtering; optional complaint items | `Reports.ViewSystemDashboard` | Preserve branch/department/mode |
| GET | `/api/anonymous-templates/{anonymousTemplateId}/responses` | Modified | ScoreCalculationMode, ScoreIncludedOnly, ComplaintItemsOnly are forwarded by controller | Same applicable fields; exact path identity retained | `AnonymousTemplates.ViewResponses` | Selected-template navigation retains its permission |
| GET | `/api/reports/branch-responses/{surveyResponseId}` | Modified | None | HistoricalSurvey, provenance/verification; historical labels/options | `Reports.ViewBranchReports` | Score remains original persisted score |
| GET | `/api/reports/system-responses/{surveyResponseId}` | Modified | None | Same | `Reports.ViewSystemDashboard` | Same |
| GET | `/api/reports/department-operators/{operatorId}/responses/{surveyResponseId}` | Modified | None | Same with existing operator/department authorization | `Reports.ViewDepartmentReports` | Same |
| GET | `/api/anonymous-templates/{anonymousTemplateId}/responses/{responseId}` | Modified | None | Same applicable historical interpretation | `AnonymousTemplates.ViewResponses` | Same |
| GET | `/api/reports/templates` | Modified | Mode 3; exact TemplateId/TemplateKind | Complaint/group analytics exposed additively; current criteria, availability and completeness | `Reports.ViewBranchReports` | Consume exact-filter results and provenance |
| GET | `/api/reports/templates/pdf` | Modified | Mode 3; exact TemplateId | Updated shared model, classification and mode/legacy explanations | `Reports.ViewBranchReports` | Preserve existing filters |
| GET | `/api/reports/templates/excel` | Modified | Mode 3; exactly one TemplateId remains required | One-template export, historical scoring, complaints and coverage | `Reports.ViewBranchReports` | Family-linked templates export separately |
| POST | `/api/operators/my-templates/{templateId}/responses` | Internal enhancement | None | Existing submission response retained | Existing Operator submission permission and validation | None |
| POST | `/api/anon-templates/{anonymousTemplateId}/responses` | Internal enhancement | None | New SingleChoice maximum is five; existing response shape retained | Existing anonymous submission rules | No payload changes |

`ComplaintItemsOnly=false` preserves ordinary response pagination. With true, rows and totals represent complaint answers; response IDs may repeat. `ScoreIncludedOnly=true` is generated for rankings so the destination count matches included score tokens. Existing `CriticalScoreThreshold=40` remains independent of satisfaction/risk settings.

## E. Database Changes

Migrations:

- `20261021110352_SurveyHistoryAndBranchEvaluation`
- `20261021113627_AddImmutableSnapshotScoreQueries`

New tables:

- BranchEvaluationSettings: unique BranchId, independent threshold order/range checks, audit metadata and restrictive branch FK.
- ResponseSnapshots: source-aware response identity, optional source-specific restrictive FKs, source check constraint, immutable JSON document, schema/provenance/capture metadata and immutable derived score/count/membership columns for all three modes.
- TemplateAvailabilityEvents: previous/new activation and scheduling states, event/source/branch/actor/timestamp/baseline metadata, template/time and branch/time indexes, and one baseline per source/template.

No historical migration was edited. No original response score column was changed. The generated upgrade script contains no response UPDATE/DELETE and no table deletion. EF reports no pending model changes.

Apply migrations through the existing deployment process. Then run bounded maintenance using the intended environment/database configuration:

```powershell
dotnet run --project CustomerSurvey.Api --no-launch-profile -- --backfill-survey-history
```

Each invocation processes up to 100 Authorized snapshots, 100 Anonymous snapshots and 100 baselines per template source. Repeat until MayHaveMore=false. Transactions are bounded and selection excludes completed records, so restart/repetition is safe. The SQL test verifies repeated runs produce no extra records. Run one maintenance worker at a time; uniqueness constraints protect identities.

Legacy snapshots are conservatively LegacyIncomplete: original answers/scores are preserved, while mutable definitions are reconstructed and never marked historically verified. Settings defaults are effective without an existing row and persisted on first update.

## F. Behavior Verification

| Behavior | Result |
|---|---|
| Strict TemplateId | Exact Authorized/Anonymous family members stay separate; branch/source validation retained |
| Excel Export | Actual workbooks generated for copied templates and historical SQL-backed data |
| Complaint Counting | Nonblank Complain answers counted independently of details; distinct response counts and rates verified |
| RootQuestions | Stable value/default 1; immutable snapshot source or identified original legacy baseline |
| LowestConditionLevel | Stable value 2; matched-path/deepest-numeric fallback verified |
| AllAnsweredQuestions | Value 3; all answered scorable branches/depths, maximum five |
| Immutable Stored Scores | Original fields unchanged after definition/settings changes; write guards added |
| Response Snapshots | Authorized/Anonymous SQL round trips, all available options, conditions, labels and media references verified |
| Current Branch Classification | Independent settings, own-branch/actor restrictions, current-threshold historical reclassification |
| NoData | No scored population produces NoData; nullable satisfaction/HasScoreData available |
| Availability History | Create/deactivate/restore/scheduling/expiry intervals, unknown legacy baseline and exact scoped counts covered |
| Best Rated Questions | Applicable dashboards, 70% minimum, answer-count/identity ties, limit and empty results covered |
| Equal Weight Per Response | Overall and group response weighting, fractional rounding and moved historical groups verified |

## G. Tests

Final build:

```powershell
dotnet build CustomerSurvey.slnx --no-restore -v quiet
```

Result: success, zero errors, 27 warnings including existing package/advisory and compiler warnings. No package/framework versions were changed.

Final focused regression run (including the opt-in isolated LocalDB test):

```powershell
$env:CUSTOMER_SURVEY_RUN_SQL_TESTS='1'
dotnet test CustomerSurvey.Tests --no-build --filter 'FullyQualifiedName~Reports|FullyQualifiedName~SurveyResponses|FullyQualifiedName~CustomInput|FullyQualifiedName~Templates' -v normal
```

Result: **173 passed, 0 failed, 0 skipped**. The entire test suite was not run.

Additional executed targeted commands during implementation:

```powershell
dotnet test CustomerSurvey.Tests --filter 'FullyQualifiedName~TemplateFamilyReportResolutionTests|FullyQualifiedName~BranchTemplateExcelReportServiceTests' --no-restore -v quiet
dotnet test CustomerSurvey.Tests --no-restore --filter 'FullyQualifiedName~HistoricalScoringAndSettingsTests|FullyQualifiedName~TemplateAvailabilityHistoryTests|FullyQualifiedName~DashboardDrillDownTests' -v normal
dotnet test CustomerSurvey.Tests --no-restore --filter 'FullyQualifiedName~TemplateFamilyReportResolutionTests|FullyQualifiedName~BranchTemplateExcelReportServiceTests|FullyQualifiedName~BranchTemplatesPdfRazorReportTests' -v normal
dotnet test CustomerSurvey.Tests --no-restore --filter 'FullyQualifiedName~SurveyHistorySqlTests' -v normal
dotnet test CustomerSurvey.Tests --no-restore --filter 'FullyQualifiedName~SurveyHistorySqlTests|FullyQualifiedName~HistoricalScoringAndSettingsTests|FullyQualifiedName~BranchTemplatesPdfRazorReportTests' -v normal
```

Successful intermediate results included 6 export tests, 43 feature/drill-down tests, 10 export regressions, the SQL persistence test, and 29 combined SQL/history/Razor tests. Counts overlap; the final 173-test run is authoritative. Earlier drill-down null-navigation failures and the legacy snapshot EF attachment-state failure were corrected and rerun successfully; a SQL fixture audit-column insert was also corrected.

Database/model verification:

```powershell
dotnet ef migrations script 20260929100536_RemoveCustomInputNameAddLabelSnapshots 20261021113627_AddImmutableSnapshotScoreQueries --idempotent --no-build --project CustomerSurvey.infrastructure --startup-project CustomerSurvey.Api --context PlatformWriteDbContext
dotnet ef migrations has-pending-model-changes --no-build --project CustomerSurvey.infrastructure --startup-project CustomerSurvey.Api --context PlatformWriteDbContext
git diff --check
```

All succeeded. The SQL test creates and deletes a unique CustomerSurveyHistoryTests_* LocalDB database. Production data was not used. PDF model/Razor rendering was verified for all three modes; the binary PDF engine was not invoked.

## H. Frontend Integration Notes

- Add the four-field settings form using the new GET/PUT routes. PUT requires the assigned BranchAdmin and seeded manage permission; retain existing access checks.
- ScoreCalculationMode values: RootQuestions=1 (default), LowestConditionLevel=2, AllAnsweredQuestions=3. Preserve mode and every generated scope/date/template filter during navigation/export.
- Read BestRatedQuestions beside LowestRatedQuestions; both use TopQuestionsCount (default five). Labels in aggregate rankings use historical submission context; individual detail snapshots retain their own full historical labels/conditions.
- Treat HasScoreData=false or nullable SatisfactionPercentage=null as N/A. Existing nonnullable AverageScorePercentage remains for compatibility and is not sufficient on its own to determine available data.
- Read Availability.AvailableTemplatesDuringPeriod, TemplatesWithResponsesCount, CurrentlyAvailableTemplatesCount, IsHistoryComplete and CoverageStartsOnUtc. When history is incomplete, period availability is the verified lower bound.
- Inspect RequestedModeAvailable, StoredBaselineResponsesCount, HasCompleteQuestionScoreData and ScoreDerivation. Legacy alternate scores are explicitly identified stored root baselines; historical question/group rankings can have partial coverage. ScoredItemsCountIsExact distinguishes uncertain legacy report item counts.
- Current classification always uses each response's originating branch. Mixed aggregate criteria use scored-response weights and expose ClassificationPolicy/HasMixedClassificationCriteria.
- Detail Score is the original persisted submission score. HistoricalSurvey is the typed historical interpretation; HistoricalDataProvenance/IsHistoricalDataVerified distinguish captured and legacy information. Raw storage payloads are not exposed in ordinary lists.
- Template selection never expands families. Excel requires exactly one selected template. Complaint totals count answers, while ResponsesWithComplaints counts distinct responses; follow generated navigation and distinguish ComplaintItemsOnly lists from response lists.
- Existing routes and submission payloads remain compatible. Approved semantic changes are exact template filtering, corrected new Anonymous SingleChoice scoring, mode-consistent analytics and current-threshold classification. New fields are additive.

## I. Remaining Issues / Future Improvements

- Production rollout must apply both migrations and run bounded history maintenance using the existing deployment process.
- Unknown pre-capture labels/options/conditions/availability cannot be verified; provenance and coverage communicate that limitation.
- Binary PDF generation was not executed; report model and Razor rendering passed. Existing PDF infrastructure remains in place.
- Existing dependency/compiler warnings remain. Package upgrades, CI/CD changes and frontend implementation were outside scope.
