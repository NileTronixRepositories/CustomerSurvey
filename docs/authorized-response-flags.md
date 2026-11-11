# Authorized response Flags and Flag Officers

## Types and fields

`Complaint = 1`, `Suggestion = 2`, and `Comment = 3`. Use those values in multipart forms. JSON responses serialize enums by name. A response can have multiple Flags of any type.

An Operator may omit `flags` entirely. Each submitted Flag requires a valid Type. Brief is optional and empty or whitespace-only values are stored as JSON `null` on reads. Non-blank Brief text has a 1,000-character limit. The multipart endpoint rejects Flag Answer fields.

A Flag Officer creating a Flag supplies both Type and a non-blank Brief. A supplied initial Answer must be non-blank and requires `ResponseFlags.Answer`. Brief edits require non-blank text up to 1,000 characters. Answer writes require non-blank text up to 4,000 characters and work when the existing Brief is null. Each edit replaces the corresponding value and records the authenticated writer and timestamp. The response, creator, creation timestamp, and Type are immutable.

## Roles and permissions

| Seeded role | Permissions for this feature |
| --- | --- |
| Flag Officer | Base role assigned during creation |
| View Response Flags | `ResponseFlags.View` |
| Create Response Flags | `ResponseFlags.Create` |
| Answer Response Flags | `ResponseFlags.Answer` |
| Edit Response Flag Briefs | `ResponseFlags.EditBrief` |
| Submit Response Flags | `ResponseFlags.CreateOnSubmit` |
| System Administrator and Department Administrator | `FlagOfficers.Create`, `FlagOfficers.ViewAll`, `FlagOfficers.ViewDetails`, `FlagOfficers.Update`, `FlagOfficers.Deactivate`, `FlagOfficers.Restore`, and `FlagOfficers.AssignRoles` |

The Department Administrator role also receives the existing `Users.ResetPassword` permission. The reset handler scopes Department Administrator access to Operator and Flag Officer accounts in the administrator’s Department. The System Administrator role receives the seeded permissions through the existing catalog. Response Flag management additionally requires a valid active Flag Officer profile.

Assign the four Flag Officer capability roles independently. An Officer with View can read flagged Authorized Responses across Branches and Departments. All Officer endpoints check the active account, actor profile, and operation-specific permission. Refresh the access token after an assignment so newly granted permissions are present in the JWT. Operator flag submission uses an independent optional role. The Operator must also have the existing submission permission.

## Operator submission and accounts

| Method and route | Permission and actor | Request and response |
| --- | --- | --- |
| `POST /api/operators/my-templates/{templateId}/responses` | `OperatorTemplates.SubmitResponse`, plus `ResponseFlags.CreateOnSubmit` when Flags are present. Active Operator required. | Adds optional multipart fields `Flags[0].Type=1` and `Flags[0].Brief`. Additional Flags use their own indices. The existing submission receipt keeps the same shape. |
| `GET /api/operators` | `Operators.ViewAll`, existing Operator administration scope | Adds `canCreateFlagsOnSubmit` to each Operator item. |
| `PUT /api/operators/{operatorId}/flag-submission` | `Operators.Update` and SuperAdmin profile | Send `{"enabled":true}` to grant the submit role or `{"enabled":false}` to revoke it. Returns the resulting boolean. |
| `POST /api/auth/login` | Existing login contract | Active Flag Officer accounts with a profile can sign in. The existing token structure carries the `FlagOfficer` user type and assigned role and permission claims. |
| `PUT /api/auth/users/{applicationUserId}/reset-password` | `Users.ResetPassword`, existing reset actor and scope checks | The existing reset contract accepts Flag Officer application user IDs. |

## Flag Officer administration

All administration routes require an active SuperAdmin or DepartmentAdmin profile. SuperAdmin supplies an active `departmentId` at creation. A DepartmentAdmin can omit it and is scoped to the Department from their profile. A request for another Department is forbidden.

| Method and route | Permission | Request and response |
| --- | --- | --- |
| `POST /api/flag-officers` | `FlagOfficers.Create` | Send `departmentId?`, `userName`, `password`, `nameEn`, `nameAr?`, `email`, and `phoneNumber?` (up to 30 characters). Returns `flagOfficerId`, `applicationUserId`, `departmentId`, and `isActive`. |
| `GET /api/flag-officers` | `FlagOfficers.ViewAll` | Supports `pageNumber`, `pageSize`, `orderSort`, `searchText`, and optional `departmentId`. Returns paginated profiles. |
| `GET /api/flag-officers/{flagOfficerId}` | `FlagOfficers.ViewDetails` | Returns the profile, Department, active state, creator ID, creation timestamp, and assigned capability roles. |
| `PUT /api/flag-officers/{flagOfficerId}` | `FlagOfficers.Update` | Send `nameEn`, `nameAr?`, `email`, and `phoneNumber?` (up to 30 characters). Returns the same fields as creation. |
| `PUT /api/flag-officers/{flagOfficerId}/deactivate` | `FlagOfficers.Deactivate` | Returns the same fields as creation with `isActive=false`. |
| `PUT /api/flag-officers/{flagOfficerId}/restore` | `FlagOfficers.Restore` | Returns the same fields as creation with `isActive=true`. |
| `GET /api/flag-officers/roles-selection` | `FlagOfficers.AssignRoles` | Returns the four capability roles with `roleId`, `name`, and `permission`. |
| `PUT /api/flag-officers/{flagOfficerId}/roles` | `FlagOfficers.AssignRoles` | Send `{"roleIds":[...]}` to replace the selected capability roles. An empty array clears them. Returns the selected roles and retains the base role. |

## Flag Officer work queue

| Method and route | Permission | Request and response |
| --- | --- | --- |
| `GET /api/response-flags/responses` | `ResponseFlags.View` | Supports `pageNumber`, `pageSize`, `orderSort`, `searchText`, `type`, `answered`, and inclusive UTC submission dates `from` and `to`. Type and answer-state filters match the same Flag. Returns one row per matching response with template, Branch, Operator, submission date, and total Flag count. |
| `GET /api/response-flags/responses/{responseId}` | `ResponseFlags.View` | Returns `response` with the original answers, custom inputs, score, and historical survey snapshot when available, plus `flags` with all associated Flags. An unflagged or unavailable response returns 404. |
| `GET /api/response-flags/{flagId}` | `ResponseFlags.View` | Returns `flagId`, `responseId`, `type`, nullable `brief` and `answer`, and creator, Brief editor, and Answer writer audit information. |
| `POST /api/response-flags/responses/{responseId}` | `ResponseFlags.Create`, plus `ResponseFlags.Answer` when an initial Answer is supplied | Send `type`, `brief`, and optional `answer`. The response must already have a Flag. Returns `flagId` and `responseId`. |
| `PUT /api/response-flags/{flagId}/brief` | `ResponseFlags.EditBrief` | Send `{"brief":"..."}`. Returns `flagId` and `responseId`. |
| `PUT /api/response-flags/{flagId}/answer` | `ResponseFlags.Answer` | Send `{"answer":"..."}`. Returns `flagId` and `responseId`. |

Use the existing pagination envelope. Validation and permission failures use the project’s existing Problem Details mapping. The Flag write receipts allow permission-specific writes. Use the View APIs to refresh Flag text and audit information.

## Dashboards and drill-downs

| Method and route | Permission | Response changes |
| --- | --- | --- |
| `GET /api/reports/branch-dashboard` | `Reports.ViewBranchReports` | Adds Flag counts to summary and template performance and adds suggestion and comment links to summary actions. |
| `GET /api/reports/department-dashboard` | `Reports.ViewDepartmentReports` | Adds Flag counts to summary, Operator activity, and template performance and adds suggestion and comment links to summary actions. |
| `GET /api/reports/system-dashboard` | `Reports.ViewSystemDashboard` | Adds Flag counts to summary, Branch and template performance, and Department activity and adds suggestion and comment links to summary actions. |
| `GET /api/reports/survey-dashboard` | `Reports.ViewBranchReports` | Adds Flag counts to summary, source breakdown, Branch summary, and template performance and adds suggestion and comment links to summary actions. |
| `GET /api/reports/branch/satisfaction` | `Reports.ViewBranchReports` | Adds Flag counts to overall and scored template totals, a distinct `responsesWithComplaints` count, and feedback drill-down links. `feedbackByTemplate` includes all templates with responses in scope and returns template identity, total `responsesCount`, and the three feedback counts. `byTemplate` continues to report scored responses and satisfaction scores. |
| `GET /api/reports/survey-dashboard/complaints` | `Reports.ViewBranchReports` | Paginates Authorized and Anonymous complaint-question answers together with Authorized Complaint Flags. The total counts items. `responsesWithComplaints` and `complaintRate` use distinct responses. |
| `GET /api/reports/survey-dashboard/suggestions` | `Reports.ViewBranchReports` | Returns `appliedFilters`, `totalItems`, `responsesWithFlags`, and `page` of Suggestion Flag details. |
| `GET /api/reports/survey-dashboard/comments` | `Reports.ViewBranchReports` | Same contract as suggestions, for Comment Flags. |
| `GET /api/reports/system-responses` | `Reports.ViewSystemDashboard` | Supports mixed complaint item mode and typed feedback item mode within existing system report scope. |
| `GET /api/reports/department-responses` | `Reports.ViewDepartmentReports` | Supports mixed complaint item mode and typed feedback item mode within the authenticated Department. |
| `GET /api/reports/branch-responses` | `Reports.ViewBranchReports` | `hasComplaint` includes Complaint Flags and existing complaint-question answers. |
| `GET /api/reports/department-operators/{operatorId}/responses` | `Reports.ViewDepartmentReports` | `hasComplaint` includes Complaint Flags and existing complaint-question answers. |
| `GET /api/reports/survey-responses` | `Reports.ViewBranchReports` | `hasComplaint` includes Complaint Flags for Authorized responses. |

System and Department response-level `hasComplaint` uses the same rule. `true` includes either source and `false` excludes both. Feedback totals count individual Flags, including Flags with a null Brief. `complaintsCount` includes existing complaint-question items. `suggestionsCount` and `commentsCount` count their respective Flags. Report dates use the response’s submission time for dashboard feedback. Satisfaction trends continue to use Visit Time. The branch satisfaction `percentageOfResponses` retains its existing complaints-per-response formula with the expanded item count.

The survey feedback endpoints use the existing Branch, source, template, date, and pagination filters. Suggestions and comments also support `answered`, `searchText` over Brief and Answer, and `flagId` for locating an individual item in scope. Each item includes read-only response navigation.

For the survey dashboard complaint list, use `itemKind` to distinguish `QuestionAnswer` from `ResponseFlag`. Flag items include `flagId`, `type`, `brief`, `answer`, `createdBy`, `briefUpdatedBy`, `answeredBy`, and audit timestamps. `questionId` is null on Flag items. `source` still identifies Internal or Anonymous responses.

The system and department response listings support `complaintItemsOnly=true` and `flagType=Suggestion` or `flagType=Comment`. Feedback item rows add `itemKind`, `flagId`, `flag`, and `detailsNavigation`. Existing response and question filters still apply. Use the response navigation returned with the item to display the original survey.

## Database

Migration `20261111143557_AddAuthorizedSurveyResponseFlags.cs` creates `FlagOfficers` and `SurveyResponseFlags` and updates the EF model snapshot. `SurveyResponseFlags.Brief` and `Answer` are nullable. The response and Type index permits repeat Types. Apply the migration and let the existing startup seeders create the roles and permissions before assigning capabilities.

## Verification

The API build passed. The focused test run passed all 52 tests, including the SQL Server LocalDB migration, role and password lifecycle, cross-Branch visibility, all five dashboards, mixed complaint pagination, Visit Time regression, and rollback and voice media compensation on failed submission. EF reported zero pending model changes.

```powershell
dotnet build CustomerSurvey.Api/CustomerSurvey.Api.csproj --no-restore --nologo -v:q -clp:ErrorsOnly

$env:CUSTOMER_SURVEY_RUN_SQL_TESTS = '1'
$flagTestClasses = @(
    'ResponseFlags', 'SurveyHistorySqlTests', 'VisitTimeReportingSqlTests',
    'DashboardDrillDownTests', 'SurveyDashboardAnalyticsEndpointTests', 'SurveyDashboardQueryTests',
    'ResetUserPasswordCommandHandlerTests', 'ImageAnswerFileValidatorTests',
    'FreeTextAndCustomInputLabelTests', 'ReportingControllerContractTests'
)
$flagTestFilter = ($flagTestClasses | ForEach-Object { "FullyQualifiedName~$_" }) -join '|'
dotnet test CustomerSurvey.Tests/CustomerSurvey.Tests.csproj --no-restore --nologo --filter $flagTestFilter -v:q --logger 'console;verbosity=minimal' -clp:ErrorsOnly

dotnet ef migrations has-pending-model-changes --project CustomerSurvey.infrastructure --startup-project CustomerSurvey.Api --context PlatformWriteDbContext --no-build
```

Database checks ran on isolated LocalDB databases. Deployment and seeding in the target environment remain unverified.

## File inventory

### Added

- `CustomerSurvey.Api/Contracts/Operators/SetOperatorFlagSubmissionRequest.cs`
- `CustomerSurvey.Api/Controllers/FlagOfficersController.cs`
- `CustomerSurvey.Api/Controllers/ResponseFlagsController.cs`
- `CustomerSurvey.Application/Features/FlagOfficers/FlagOfficerAdministrationScope.cs`
- `CustomerSurvey.Application/Features/FlagOfficers/FlagOfficerCommandHandlers.cs`
- `CustomerSurvey.Application/Features/FlagOfficers/FlagOfficerQueryHandlers.cs`
- `CustomerSurvey.Application/Features/FlagOfficers/FlagOfficerRequests.cs`
- `CustomerSurvey.Application/Features/FlagOfficers/FlagOfficerSpecifications.cs`
- `CustomerSurvey.Application/Features/FlagOfficers/FlagOfficerValidation.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboardComplaints/GetSurveyDashboardFlagFeedbackQuery.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponseDetails/AuthorizedResponseDetailsReader.cs`
- `CustomerSurvey.Application/Features/Reports/Shared/AuthorizedFeedbackItems.cs`
- `CustomerSurvey.Application/Features/ResponseFlags/FlagOfficerAccess.cs`
- `CustomerSurvey.Application/Features/ResponseFlags/ResponseFlagCommandHandlers.cs`
- `CustomerSurvey.Application/Features/ResponseFlags/ResponseFlagQueryHandlers.cs`
- `CustomerSurvey.Application/Features/ResponseFlags/ResponseFlagRequests.cs`
- `CustomerSurvey.Application/Features/ResponseFlags/ResponseFlagSpecifications.cs`
- `CustomerSurvey.Application/Features/ResponseFlags/ResponseFlagValidation.cs`
- `CustomerSurvey.Application/Shared/FlagPermissions.cs`
- `CustomerSurvey.Domain/Entities/SurveyResponseFlag.cs`
- `CustomerSurvey.Domain/Enums/SurveyResponseFlagType.cs`
- `CustomerSurvey.Domain/Identity/FlagAccess.cs`
- `CustomerSurvey.Domain/Identity/FlagOfficer.cs`
- `CustomerSurvey.Tests/ResponseFlags/ResponseFlagValidationTests.cs`
- `CustomerSurvey.Tests/ResponseFlags/ResponseFlagsSqlTests.cs`
- `CustomerSurvey.infrastructure/Configurations/FlagOfficerConfiguration.cs`
- `CustomerSurvey.infrastructure/Configurations/SurveyResponseFlagConfiguration.cs`
- `CustomerSurvey.infrastructure/Migrations/20261111143557_AddAuthorizedSurveyResponseFlags.Designer.cs`
- `CustomerSurvey.infrastructure/Migrations/20261111143557_AddAuthorizedSurveyResponseFlags.cs`
- `docs/authorized-response-flags.md`

### Updated

- `CustomerSurvey.Api/Contracts/Operators/SubmitOperatorTemplateResponseRequest.cs`
- `CustomerSurvey.Api/Controllers/OperatorsController.cs`
- `CustomerSurvey.Api/Controllers/ReportsController.cs`
- `CustomerSurvey.Application/Abstraction/Reports/ISurveyDashboardComplaintReadService.cs`
- `CustomerSurvey.Application/Bootstrap/Bootstrap.cs`
- `CustomerSurvey.Application/Features/Auth/Command/Login/LoginCommandHandler.cs`
- `CustomerSurvey.Application/Features/Auth/Command/ResetUserPassword/ResetUserPasswordCommandHandler.cs`
- `CustomerSurvey.Application/Features/Operators/Query/GetOperatorsPagination/GetOperatorsPaginationQueryHandler.cs`
- `CustomerSurvey.Application/Features/Operators/Query/GetOperatorsPagination/GetOperatorsPaginationSpec.cs`
- `CustomerSurvey.Application/Features/Operators/Query/GetOperatorsPagination/OperatorPaginationItemDto.cs`
- `CustomerSurvey.Application/Features/Operators/Query/GetOperatorsPagination/OperatorPaginationItemResponse.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/DashboardSurveyResponseDto.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardQueryHandler.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetBranchDashboardResponse.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetBranchDashboard/GetDashboardSurveyResponsesSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportQueryHandler.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetBranchSatisfactionReportResponse.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/GetSatisfactionSurveyResponsesForBranchReportSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetBranchSatisfactionReport/SatisfactionSurveyResponseFlatDto.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetBranchSurveyResponsesPagination/GetBranchSurveyResponsesPaginationSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/DepartmentDashboardSurveyResponseDto.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardQueryHandler.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardResponse.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentDashboard/GetDepartmentDashboardSurveyResponsesSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentOperatorSurveyResponsesPagination/GetDepartmentOperatorSurveyResponsesPaginationSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/DepartmentSurveyResponsePaginationItemResponse.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationQuery.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationQueryHandler.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationQueryValidator.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetDepartmentSurveyResponsesPagination/GetDepartmentSurveyResponsesPaginationSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardInternalResponsesSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardQueryHandler.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboard/GetSurveyDashboardResponse.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboardComplaints/GetSurveyDashboardComplaintsQuery.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboardComplaints/GetSurveyDashboardComplaintsQueryHandler.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboardComplaints/GetSurveyDashboardComplaintsQueryValidator.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSurveyDashboardComplaints/SurveyDashboardComplaintsResponse.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardQueryHandler.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardResponse.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/GetSystemDashboardSurveyResponsesSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemDashboard/SystemDashboardSurveyResponseDto.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponseDetails/GetSystemSurveyResponseDetailsBasicSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponseDetails/GetSystemSurveyResponseDetailsQueryHandler.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationQuery.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationQueryHandler.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationQueryValidator.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/GetSystemSurveyResponsesPaginationSpec.cs`
- `CustomerSurvey.Application/Features/Reports/Query/GetSystemSurveyResponsesPagination/SystemSurveyResponsePaginationItemResponse.cs`
- `CustomerSurvey.Application/Features/Reports/Shared/ComplaintItemsSpecification.cs`
- `CustomerSurvey.Application/Features/Reports/Shared/DashboardNavigation.cs`
- `CustomerSurvey.Application/Features/SurveyResponses/Command/SubmitOperatorTemplateResponse/GetCurrentOperatorForSubmitResponseSpec.cs`
- `CustomerSurvey.Application/Features/SurveyResponses/Command/SubmitOperatorTemplateResponse/SubmitOperatorTemplateResponseCommand.cs`
- `CustomerSurvey.Application/Features/SurveyResponses/Command/SubmitOperatorTemplateResponse/SubmitOperatorTemplateResponseCommandHandler.cs`
- `CustomerSurvey.Application/Features/SurveyResponses/Command/SubmitOperatorTemplateResponse/SubmitOperatorTemplateResponseCommandValidator.cs`
- `CustomerSurvey.Domain/Entities/SurveyResponse.cs`
- `CustomerSurvey.Domain/Enums/UserType.cs`
- `CustomerSurvey.Tests/Auth/ResetUserPasswordCommandHandlerTests.cs`
- `CustomerSurvey.Tests/Reports/SurveyHistorySqlTests.cs`
- `CustomerSurvey.infrastructure/Migrations/PlatformWriteDbContextModelSnapshot.cs`
- `CustomerSurvey.infrastructure/Reports/SurveyDashboardComplaintReadService.cs`
- `CustomerSurvey.infrastructure/Seeders/SeedConstants.cs`
