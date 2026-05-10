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
    }

    public static class PermissionNames
    {
        public const string BranchesCreate = "Branches.Create";
        public const string BranchesViewAll = "Branches.ViewAll";
        public const string BranchesViewDetails = "Branches.ViewDetails";

        public const string BranchAdminsCreate = "BranchAdmins.Create";
        public const string BranchAdminsViewAll = "BranchAdmins.ViewAll";
        public const string BranchesUpdate = "Branches.Update";

        public const string DepartmentsCreate = "Departments.Create";
        public const string DepartmentsViewAll = "Departments.ViewAll";
        public const string DepartmentsViewSelection = "Departments.ViewSelection";

        public const string DepartmentAdminsCreate = "DepartmentAdmins.Create";
        public const string DepartmentAdminsViewAll = "DepartmentAdmins.ViewAll";

        public const string BranchUsersCreate = "BranchUsers.Create";
        public const string BranchUsersViewAll = "BranchUsers.ViewAll";
        public const string BranchUsersAssignRoles = "BranchUsers.AssignRoles";

        public const string RolesViewSelection = "Roles.ViewSelection";

        public const string TemplatesCreate = "Templates.Create";
        public const string TemplatesUpdate = "Templates.Update";
        public const string TemplatesDelete = "Templates.Delete";
        public const string TemplatesViewAll = "Templates.ViewAll";
        public const string TemplatesViewDetails = "Templates.ViewDetails";
        public const string TemplatesViewSelection = "Templates.ViewSelection";
        public const string TemplatesAssignQuestions = "Templates.AssignQuestions";

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

        public const string ReportsViewBranchReports = "Reports.ViewBranchReports";
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
    new RoleSeedItem(SeedIds.Roles.QuestionEditor, RoleNames.QuestionEditor)
        ];

        public static readonly IReadOnlyCollection<PermissionSeedItem> Permissions =
        [
            new PermissionSeedItem(SeedIds.Permissions.BranchesCreate, PermissionNames.BranchesCreate),
            new PermissionSeedItem(SeedIds.Permissions.BranchesViewAll, PermissionNames.BranchesViewAll),
            new PermissionSeedItem(SeedIds.Permissions.BranchesViewDetails, PermissionNames.BranchesViewDetails),

            new PermissionSeedItem(SeedIds.Permissions.BranchAdminsCreate, PermissionNames.BranchAdminsCreate),
            new PermissionSeedItem(SeedIds.Permissions.BranchAdminsViewAll, PermissionNames.BranchAdminsViewAll),
            new PermissionSeedItem(SeedIds.Permissions.BranchesUpdate, PermissionNames.BranchesUpdate),

            new PermissionSeedItem(SeedIds.Permissions.DepartmentsCreate, PermissionNames.DepartmentsCreate),
            new PermissionSeedItem(SeedIds.Permissions.DepartmentsViewAll, PermissionNames.DepartmentsViewAll),
            new PermissionSeedItem(SeedIds.Permissions.DepartmentsViewSelection, PermissionNames.DepartmentsViewSelection),

            new PermissionSeedItem(SeedIds.Permissions.DepartmentAdminsCreate, PermissionNames.DepartmentAdminsCreate),
            new PermissionSeedItem(SeedIds.Permissions.DepartmentAdminsViewAll, PermissionNames.DepartmentAdminsViewAll),

            new PermissionSeedItem(SeedIds.Permissions.BranchUsersCreate, PermissionNames.BranchUsersCreate),
            new PermissionSeedItem(SeedIds.Permissions.BranchUsersViewAll, PermissionNames.BranchUsersViewAll),
            new PermissionSeedItem(SeedIds.Permissions.BranchUsersAssignRoles, PermissionNames.BranchUsersAssignRoles),

            new PermissionSeedItem(SeedIds.Permissions.RolesViewSelection, PermissionNames.RolesViewSelection),

            new PermissionSeedItem(SeedIds.Permissions.TemplatesCreate, PermissionNames.TemplatesCreate),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesUpdate, PermissionNames.TemplatesUpdate),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesDelete, PermissionNames.TemplatesDelete),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesViewAll, PermissionNames.TemplatesViewAll),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesViewDetails, PermissionNames.TemplatesViewDetails),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesViewSelection, PermissionNames.TemplatesViewSelection),
            new PermissionSeedItem(SeedIds.Permissions.TemplatesAssignQuestions, PermissionNames.TemplatesAssignQuestions),

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

            new PermissionSeedItem(SeedIds.Permissions.ReportsViewBranchReports, PermissionNames.ReportsViewBranchReports)
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
                SeedIds.Permissions.DepartmentsCreate,
                SeedIds.Permissions.DepartmentsViewAll,
                SeedIds.Permissions.DepartmentsViewSelection,

                SeedIds.Permissions.DepartmentAdminsCreate,
                SeedIds.Permissions.DepartmentAdminsViewAll,

                SeedIds.Permissions.BranchUsersCreate,
                SeedIds.Permissions.BranchUsersViewAll,
                SeedIds.Permissions.BranchUsersAssignRoles,

                SeedIds.Permissions.RolesViewSelection,

                SeedIds.Permissions.TemplatesCreate,
                SeedIds.Permissions.TemplatesUpdate,
                SeedIds.Permissions.TemplatesDelete,
                SeedIds.Permissions.TemplatesViewAll,
                SeedIds.Permissions.TemplatesViewDetails,
                SeedIds.Permissions.TemplatesViewSelection,
                SeedIds.Permissions.TemplatesAssignQuestions,

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
                SeedIds.Permissions.TemplatesAssignQuestions
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

        private static void AddDepartmentAdministratorPermissions(List<RolePermissionSeedItem> result)
        {
            var roleId = SeedIds.Roles.DepartmentAdministrator;

            Guid[] permissionIds =
            [
                SeedIds.Permissions.OperatorsCreate,
        SeedIds.Permissions.OperatorsViewAll,
        SeedIds.Permissions.OperatorsAssignTemplates,
        SeedIds.Permissions.TemplatesViewSelection
            ];

            foreach (var permissionId in permissionIds)
            {
                result.Add(new RolePermissionSeedItem(roleId, permissionId));
            }
        }
    }
}