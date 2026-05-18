using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomerSurvey.infrastructure.Seeders;

internal static class SeedConstants
{
    public static class SuperAdminSeed
    {
        public static readonly Guid ApplicationUserId =
            Guid.Parse("10000000-0000-0000-0000-000000000001");

        public static readonly Guid SuperAdminId =
            Guid.Parse("10000000-0000-0000-0000-000000000002");

        public const string Email = "superadmin@customersurvey.local";
        public const string UserName = "superadmin";
        public const string NameEn = "System Super Admin";

        // Development only.
        public const string DefaultPassword = "Admin@123456";
    }

    public static class RoleNames
    {
        public const string SystemAdministrator = "System Administrator";
        public const string BranchAdministrator = "Branch Administrator";
        public const string TemplateEditor = "Template Editor";
        public const string ReportViewer = "Report Viewer";
        public const string QuestionEditor = "Question Editor";
        public const string DepartmentAdministrator = "Department Administrator";
        public const string Operator = "Operator";
    }

    public static class PermissionNames
    {
        public const string BranchesCreate = "Branches.Create";
        public const string BranchesViewAll = "Branches.ViewAll";
        public const string BranchesViewDetails = "Branches.ViewDetails";
        public const string BranchesUpdate = "Branches.Update";

        public const string BranchAdminsCreate = "BranchAdmins.Create";
        public const string BranchAdminsViewAll = "BranchAdmins.ViewAll";

        public const string DepartmentsCreate = "Departments.Create";
        public const string DepartmentsViewAll = "Departments.ViewAll";
        public const string DepartmentsViewSelection = "Departments.ViewSelection";
        public const string DepartmentsUpdate = "Departments.Update";
        public const string DepartmentsDelete = "Departments.Delete";

        public const string DepartmentAdminsCreate = "DepartmentAdmins.Create";
        public const string DepartmentAdminsViewAll = "DepartmentAdmins.ViewAll";

        public const string BranchUsersCreate = "BranchUsers.Create";
        public const string BranchUsersViewAll = "BranchUsers.ViewAll";
        public const string BranchUsersAssignRoles = "BranchUsers.AssignRoles";
        public const string BranchUsersUpdate = "BranchUsers.Update";
        public const string BranchUsersDelete = "BranchUsers.Delete";
        public const string BranchUsersResetPassword = "BranchUsers.ResetPassword";

        public const string RolesViewSelection = "Roles.ViewSelection";

        public const string TemplatesCreate = "Templates.Create";
        public const string TemplatesUpdate = "Templates.Update";
        public const string TemplatesDelete = "Templates.Delete";
        public const string TemplatesViewAll = "Templates.ViewAll";
        public const string TemplatesViewDetails = "Templates.ViewDetails";
        public const string TemplatesViewSelection = "Templates.ViewSelection";
        public const string TemplatesAssignQuestions = "Templates.AssignQuestions";
        public const string TemplatesManageQuestionConditions = "Templates.ManageQuestionConditions";

        public const string QuestionGroupsCreate = "QuestionGroups.Create";
        public const string QuestionGroupsUpdate = "QuestionGroups.Update";
        public const string QuestionGroupsDelete = "QuestionGroups.Delete";
        public const string QuestionGroupsViewAll = "QuestionGroups.ViewAll";

        public const string QuestionsCreate = "Questions.Create";
        public const string QuestionsUpdate = "Questions.Update";
        public const string QuestionsDelete = "Questions.Delete";
        public const string QuestionsViewAll = "Questions.ViewAll";

        public const string OperatorsCreate = "Operators.Create";
        public const string OperatorsViewAll = "Operators.ViewAll";
        public const string OperatorsAssignTemplates = "Operators.AssignTemplates";
        public const string OperatorsUpdate = "Operators.Update";

        public const string OperatorTemplatesViewMine = "OperatorTemplates.ViewMine";
        public const string OperatorTemplatesSubmitResponse = "OperatorTemplates.SubmitResponse";

        public const string ReportsViewBranchReports = "Reports.ViewBranchReports";

        public const string GlobalQuestionGroupsCreate = "GlobalQuestionGroups.Create";
        public const string GlobalQuestionGroupsUpdate = "GlobalQuestionGroups.Update";
        public const string GlobalQuestionGroupsDelete = "GlobalQuestionGroups.Delete";
        public const string GlobalQuestionGroupsViewAll = "GlobalQuestionGroups.ViewAll";
        public const string GlobalQuestionGroupsRestore = "GlobalQuestionGroups.Restore";

        public const string GlobalQuestionsCreate = "GlobalQuestions.Create";
        public const string GlobalQuestionsUpdate = "GlobalQuestions.Update";
        public const string GlobalQuestionsDelete = "GlobalQuestions.Delete";
        public const string GlobalQuestionsViewAll = "GlobalQuestions.ViewAll";
        public const string GlobalQuestionsRestore = "GlobalQuestions.Restore";
    }

    public static class SeedIds
    {
        public static class Roles
        {
            public static readonly Guid SystemAdministrator =
                Guid.Parse("11000000-0000-0000-0000-000000000001");

            public static readonly Guid BranchAdministrator =
                Guid.Parse("11000000-0000-0000-0000-000000000002");

            public static readonly Guid TemplateEditor =
                Guid.Parse("11000000-0000-0000-0000-000000000003");

            public static readonly Guid ReportViewer =
                Guid.Parse("11000000-0000-0000-0000-000000000004");

            public static readonly Guid QuestionEditor =
                Guid.Parse("11000000-0000-0000-0000-000000000005");

            public static readonly Guid DepartmentAdministrator =
                Guid.Parse("11000000-0000-0000-0000-000000000006");

            public static readonly Guid Operator =
                Guid.Parse("11000000-0000-0000-0000-000000000007");
        }

        public static class Permissions
        {
            public static readonly Guid BranchesCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000001");

            public static readonly Guid BranchesViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000002");

            public static readonly Guid BranchesViewDetails =
                Guid.Parse("12000000-0000-0000-0000-000000000003");

            public static readonly Guid BranchAdminsCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000004");

            public static readonly Guid BranchAdminsViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000005");

            public static readonly Guid DepartmentsCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000006");

            public static readonly Guid DepartmentsViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000007");

            public static readonly Guid DepartmentsViewSelection =
                Guid.Parse("12000000-0000-0000-0000-000000000008");

            public static readonly Guid DepartmentAdminsCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000009");

            public static readonly Guid DepartmentAdminsViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000010");

            public static readonly Guid BranchUsersCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000011");

            public static readonly Guid BranchUsersViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000012");

            public static readonly Guid BranchUsersAssignRoles =
                Guid.Parse("12000000-0000-0000-0000-000000000013");

            public static readonly Guid RolesViewSelection =
                Guid.Parse("12000000-0000-0000-0000-000000000014");

            public static readonly Guid TemplatesCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000015");

            public static readonly Guid TemplatesUpdate =
                Guid.Parse("12000000-0000-0000-0000-000000000016");

            public static readonly Guid TemplatesDelete =
                Guid.Parse("12000000-0000-0000-0000-000000000017");

            public static readonly Guid TemplatesViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000018");

            public static readonly Guid TemplatesViewDetails =
                Guid.Parse("12000000-0000-0000-0000-000000000019");

            public static readonly Guid TemplatesViewSelection =
                Guid.Parse("12000000-0000-0000-0000-000000000020");

            public static readonly Guid TemplatesAssignQuestions =
                Guid.Parse("12000000-0000-0000-0000-000000000021");

            public static readonly Guid QuestionGroupsCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000022");

            public static readonly Guid QuestionGroupsUpdate =
                Guid.Parse("12000000-0000-0000-0000-000000000023");

            public static readonly Guid QuestionGroupsDelete =
                Guid.Parse("12000000-0000-0000-0000-000000000024");

            public static readonly Guid QuestionGroupsViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000025");

            public static readonly Guid QuestionsCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000026");

            public static readonly Guid QuestionsUpdate =
                Guid.Parse("12000000-0000-0000-0000-000000000027");

            public static readonly Guid QuestionsDelete =
                Guid.Parse("12000000-0000-0000-0000-000000000028");

            public static readonly Guid QuestionsViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000029");

            public static readonly Guid OperatorsCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000030");

            public static readonly Guid OperatorsViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000031");

            public static readonly Guid OperatorsAssignTemplates =
                Guid.Parse("12000000-0000-0000-0000-000000000032");

            public static readonly Guid ReportsViewBranchReports =
                Guid.Parse("12000000-0000-0000-0000-000000000033");

            public static readonly Guid BranchesUpdate =
                Guid.Parse("12000000-0000-0000-0000-000000000034");

            public static readonly Guid OperatorTemplatesViewMine =
                Guid.Parse("12000000-0000-0000-0000-000000000035");

            public static readonly Guid DepartmentsUpdate =
                Guid.Parse("12000000-0000-0000-0000-000000000036");

            public static readonly Guid DepartmentsDelete =
                Guid.Parse("12000000-0000-0000-0000-000000000037");

            public static readonly Guid OperatorsUpdate =
                Guid.Parse("12000000-0000-0000-0000-000000000038");

            public static readonly Guid OperatorTemplatesSubmitResponse =
                Guid.Parse("12000000-0000-0000-0000-000000000039");

            public static readonly Guid BranchUsersUpdate =
                Guid.Parse("12000000-0000-0000-0000-000000000040");

            public static readonly Guid BranchUsersDelete =
                Guid.Parse("12000000-0000-0000-0000-000000000041");

            public static readonly Guid BranchUsersResetPassword =
                Guid.Parse("12000000-0000-0000-0000-000000000042");

            public static readonly Guid TemplatesManageQuestionConditions =
                Guid.Parse("12000000-0000-0000-0000-000000000043");

            public static readonly Guid GlobalQuestionGroupsCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000050");

            public static readonly Guid GlobalQuestionGroupsUpdate =
                Guid.Parse("12000000-0000-0000-0000-000000000051");

            public static readonly Guid GlobalQuestionGroupsDelete =
                Guid.Parse("12000000-0000-0000-0000-000000000052");

            public static readonly Guid GlobalQuestionGroupsViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000053");

            public static readonly Guid GlobalQuestionGroupsRestore =
                Guid.Parse("12000000-0000-0000-0000-000000000054");

            public static readonly Guid GlobalQuestionsCreate =
                Guid.Parse("12000000-0000-0000-0000-000000000055");

            public static readonly Guid GlobalQuestionsUpdate =
                Guid.Parse("12000000-0000-0000-0000-000000000056");

            public static readonly Guid GlobalQuestionsDelete =
                Guid.Parse("12000000-0000-0000-0000-000000000057");

            public static readonly Guid GlobalQuestionsViewAll =
                Guid.Parse("12000000-0000-0000-0000-000000000058");

            public static readonly Guid GlobalQuestionsRestore =
                Guid.Parse("12000000-0000-0000-0000-000000000059");
        }
    }

    public sealed record RoleSeedItem(Guid Id, string Name);

    public sealed record PermissionSeedItem(Guid Id, string Name);

    public sealed record RolePermissionSeedItem(Guid RoleId, Guid PermissionId);

    public static class SeedCatalog
    {
        public static readonly IReadOnlyCollection<RoleSeedItem> Roles =
        [
            new RoleSeedItem(SeedIds.Roles.SystemAdministrator, RoleNames.SystemAdministrator),
            new RoleSeedItem(SeedIds.Roles.BranchAdministrator, RoleNames.BranchAdministrator),
            new RoleSeedItem(SeedIds.Roles.DepartmentAdministrator, RoleNames.DepartmentAdministrator),
            new RoleSeedItem(SeedIds.Roles.TemplateEditor, RoleNames.TemplateEditor),
            new RoleSeedItem(SeedIds.Roles.ReportViewer, RoleNames.ReportViewer),
            new RoleSeedItem(SeedIds.Roles.QuestionEditor, RoleNames.QuestionEditor),
            new RoleSeedItem(SeedIds.Roles.Operator, RoleNames.Operator)
        ];

        public static readonly IReadOnlyCollection<PermissionSeedItem> Permissions =
        [
            new PermissionSeedItem(SeedIds.Permissions.BranchesCreate, PermissionNames.BranchesCreate),
            new PermissionSeedItem(SeedIds.Permissions.BranchesViewAll, PermissionNames.BranchesViewAll),
            new PermissionSeedItem(SeedIds.Permissions.BranchesViewDetails, PermissionNames.BranchesViewDetails),
            new PermissionSeedItem(SeedIds.Permissions.BranchesUpdate, PermissionNames.BranchesUpdate),

            new PermissionSeedItem(SeedIds.Permissions.BranchAdminsCreate, PermissionNames.BranchAdminsCreate),
            new PermissionSeedItem(SeedIds.Permissions.BranchAdminsViewAll, PermissionNames.BranchAdminsViewAll),

            new PermissionSeedItem(SeedIds.Permissions.DepartmentsCreate, PermissionNames.DepartmentsCreate),
            new PermissionSeedItem(SeedIds.Permissions.DepartmentsViewAll, PermissionNames.DepartmentsViewAll),
            new PermissionSeedItem(SeedIds.Permissions.DepartmentsViewSelection, PermissionNames.DepartmentsViewSelection),
            new PermissionSeedItem(SeedIds.Permissions.DepartmentsUpdate, PermissionNames.DepartmentsUpdate),
            new PermissionSeedItem(SeedIds.Permissions.DepartmentsDelete, PermissionNames.DepartmentsDelete),

            new PermissionSeedItem(SeedIds.Permissions.DepartmentAdminsCreate, PermissionNames.DepartmentAdminsCreate),
            new PermissionSeedItem(SeedIds.Permissions.DepartmentAdminsViewAll, PermissionNames.DepartmentAdminsViewAll),

            new PermissionSeedItem(SeedIds.Permissions.BranchUsersCreate, PermissionNames.BranchUsersCreate),
            new PermissionSeedItem(SeedIds.Permissions.BranchUsersViewAll, PermissionNames.BranchUsersViewAll),
            new PermissionSeedItem(SeedIds.Permissions.BranchUsersAssignRoles, PermissionNames.BranchUsersAssignRoles),
            new PermissionSeedItem(SeedIds.Permissions.BranchUsersUpdate, PermissionNames.BranchUsersUpdate),
            new PermissionSeedItem(SeedIds.Permissions.BranchUsersDelete, PermissionNames.BranchUsersDelete),
            new PermissionSeedItem(SeedIds.Permissions.BranchUsersResetPassword, PermissionNames.BranchUsersResetPassword),

            new PermissionSeedItem(SeedIds.Permissions.RolesViewSelection, PermissionNames.RolesViewSelection),

            new PermissionSeedItem(SeedIds.Permissions.TemplatesCreate, PermissionNames.TemplatesCreate),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesUpdate, PermissionNames.TemplatesUpdate),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesDelete, PermissionNames.TemplatesDelete),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesViewAll, PermissionNames.TemplatesViewAll),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesViewDetails, PermissionNames.TemplatesViewDetails),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesViewSelection, PermissionNames.TemplatesViewSelection),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesAssignQuestions, PermissionNames.TemplatesAssignQuestions),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesManageQuestionConditions, PermissionNames.TemplatesManageQuestionConditions),

            new PermissionSeedItem(SeedIds.Permissions.QuestionGroupsCreate, PermissionNames.QuestionGroupsCreate),
            new PermissionSeedItem(SeedIds.Permissions.QuestionGroupsUpdate, PermissionNames.QuestionGroupsUpdate),
            new PermissionSeedItem(SeedIds.Permissions.QuestionGroupsDelete, PermissionNames.QuestionGroupsDelete),
            new PermissionSeedItem(SeedIds.Permissions.QuestionGroupsViewAll, PermissionNames.QuestionGroupsViewAll),

            new PermissionSeedItem(SeedIds.Permissions.QuestionsCreate, PermissionNames.QuestionsCreate),
            new PermissionSeedItem(SeedIds.Permissions.QuestionsUpdate, PermissionNames.QuestionsUpdate),
            new PermissionSeedItem(SeedIds.Permissions.QuestionsDelete, PermissionNames.QuestionsDelete),
            new PermissionSeedItem(SeedIds.Permissions.QuestionsViewAll, PermissionNames.QuestionsViewAll),

            new PermissionSeedItem(SeedIds.Permissions.OperatorsCreate, PermissionNames.OperatorsCreate),
            new PermissionSeedItem(SeedIds.Permissions.OperatorsViewAll, PermissionNames.OperatorsViewAll),
            new PermissionSeedItem(SeedIds.Permissions.OperatorsAssignTemplates, PermissionNames.OperatorsAssignTemplates),
            new PermissionSeedItem(SeedIds.Permissions.OperatorsUpdate, PermissionNames.OperatorsUpdate),

            new PermissionSeedItem(SeedIds.Permissions.OperatorTemplatesViewMine, PermissionNames.OperatorTemplatesViewMine),
            new PermissionSeedItem(SeedIds.Permissions.OperatorTemplatesSubmitResponse, PermissionNames.OperatorTemplatesSubmitResponse),

            new PermissionSeedItem(SeedIds.Permissions.ReportsViewBranchReports, PermissionNames.ReportsViewBranchReports),

            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionGroupsCreate, PermissionNames.GlobalQuestionGroupsCreate),
            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionGroupsUpdate, PermissionNames.GlobalQuestionGroupsUpdate),
            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionGroupsDelete, PermissionNames.GlobalQuestionGroupsDelete),
            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionGroupsViewAll, PermissionNames.GlobalQuestionGroupsViewAll),
            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionGroupsRestore, PermissionNames.GlobalQuestionGroupsRestore),

            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionsCreate, PermissionNames.GlobalQuestionsCreate),
            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionsUpdate, PermissionNames.GlobalQuestionsUpdate),
            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionsDelete, PermissionNames.GlobalQuestionsDelete),
            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionsViewAll, PermissionNames.GlobalQuestionsViewAll),
            new PermissionSeedItem(SeedIds.Permissions.GlobalQuestionsRestore, PermissionNames.GlobalQuestionsRestore)
        ];

        public static readonly IReadOnlyCollection<RolePermissionSeedItem> RolePermissions =
            BuildRolePermissions();

        private static IReadOnlyCollection<RolePermissionSeedItem> BuildRolePermissions()
        {
            var result = new List<RolePermissionSeedItem>();

            AddSystemAdministratorPermissions(result);
            AddBranchAdministratorPermissions(result);
            AddDepartmentAdministratorPermissions(result);
            AddTemplateEditorPermissions(result);
            AddQuestionEditorPermissions(result);
            AddReportViewerPermissions(result);
            AddOperatorPermissions(result);

            return result
                .Distinct()
                .ToArray();
        }

        private static void AddSystemAdministratorPermissions(List<RolePermissionSeedItem> result)
        {
            foreach (var permission in Permissions)
            {
                result.Add(new RolePermissionSeedItem(
                    SeedIds.Roles.SystemAdministrator,
                    permission.Id));
            }
        }

        private static void AddBranchAdministratorPermissions(List<RolePermissionSeedItem> result)
        {
            var roleId = SeedIds.Roles.BranchAdministrator;

            Guid[] permissionIds =
            [
                SeedIds.Permissions.BranchesViewDetails,

                SeedIds.Permissions.BranchUsersCreate,
                SeedIds.Permissions.BranchUsersViewAll,
                SeedIds.Permissions.BranchUsersAssignRoles,
                SeedIds.Permissions.BranchUsersUpdate,
                SeedIds.Permissions.BranchUsersDelete,
                SeedIds.Permissions.BranchUsersResetPassword,

                SeedIds.Permissions.RolesViewSelection,

                SeedIds.Permissions.TemplatesCreate,
                SeedIds.Permissions.TemplatesUpdate,
                SeedIds.Permissions.TemplatesDelete,
                SeedIds.Permissions.TemplatesViewAll,
                SeedIds.Permissions.TemplatesViewDetails,
                SeedIds.Permissions.TemplatesViewSelection,
                SeedIds.Permissions.TemplatesAssignQuestions,
                SeedIds.Permissions.TemplatesManageQuestionConditions,

                SeedIds.Permissions.QuestionGroupsCreate,
                SeedIds.Permissions.QuestionGroupsUpdate,
                SeedIds.Permissions.QuestionGroupsDelete,
                SeedIds.Permissions.QuestionGroupsViewAll,

                SeedIds.Permissions.QuestionsCreate,
                SeedIds.Permissions.QuestionsUpdate,
                SeedIds.Permissions.QuestionsDelete,
                SeedIds.Permissions.QuestionsViewAll,

                SeedIds.Permissions.ReportsViewBranchReports
            ];

            foreach (var permissionId in permissionIds)
            {
                result.Add(new RolePermissionSeedItem(roleId, permissionId));
            }
        }

        private static void AddDepartmentAdministratorPermissions(List<RolePermissionSeedItem> result)
        {
            var roleId = SeedIds.Roles.DepartmentAdministrator;

            Guid[] permissionIds =
            [
                SeedIds.Permissions.OperatorsCreate,
                SeedIds.Permissions.OperatorsViewAll,
                SeedIds.Permissions.OperatorsAssignTemplates,
                SeedIds.Permissions.OperatorsUpdate,
                SeedIds.Permissions.TemplatesViewSelection
            ];

            foreach (var permissionId in permissionIds)
            {
                result.Add(new RolePermissionSeedItem(roleId, permissionId));
            }
        }

        private static void AddTemplateEditorPermissions(List<RolePermissionSeedItem> result)
        {
            var roleId = SeedIds.Roles.TemplateEditor;

            Guid[] permissionIds =
            [
                SeedIds.Permissions.TemplatesCreate,
                SeedIds.Permissions.TemplatesUpdate,
                SeedIds.Permissions.TemplatesDelete,
                SeedIds.Permissions.TemplatesViewAll,
                SeedIds.Permissions.TemplatesViewDetails,
                SeedIds.Permissions.TemplatesViewSelection,
                SeedIds.Permissions.TemplatesAssignQuestions,
                SeedIds.Permissions.TemplatesManageQuestionConditions
            ];

            foreach (var permissionId in permissionIds)
            {
                result.Add(new RolePermissionSeedItem(roleId, permissionId));
            }
        }

        private static void AddQuestionEditorPermissions(List<RolePermissionSeedItem> result)
        {
            var roleId = SeedIds.Roles.QuestionEditor;

            Guid[] permissionIds =
            [
                SeedIds.Permissions.QuestionGroupsCreate,
                SeedIds.Permissions.QuestionGroupsUpdate,
                SeedIds.Permissions.QuestionGroupsDelete,
                SeedIds.Permissions.QuestionGroupsViewAll,

                SeedIds.Permissions.QuestionsCreate,
                SeedIds.Permissions.QuestionsUpdate,
                SeedIds.Permissions.QuestionsDelete,
                SeedIds.Permissions.QuestionsViewAll
            ];

            foreach (var permissionId in permissionIds)
            {
                result.Add(new RolePermissionSeedItem(roleId, permissionId));
            }
        }

        private static void AddReportViewerPermissions(List<RolePermissionSeedItem> result)
        {
            result.Add(new RolePermissionSeedItem(
                SeedIds.Roles.ReportViewer,
                SeedIds.Permissions.ReportsViewBranchReports));
        }

        private static void AddOperatorPermissions(List<RolePermissionSeedItem> result)
        {
            var roleId = SeedIds.Roles.Operator;

            Guid[] permissionIds =
            [
                SeedIds.Permissions.OperatorTemplatesViewMine,
                SeedIds.Permissions.OperatorTemplatesSubmitResponse
            ];

            foreach (var permissionId in permissionIds)
            {
                result.Add(new RolePermissionSeedItem(roleId, permissionId));
            }
        }
    }
}