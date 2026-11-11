namespace CustomerSurvey.Domain.Identity;

public static class FlagAccess
{
    public const string ResponseFlagsCreateOnSubmit = "ResponseFlags.CreateOnSubmit";
    public const string ResponseFlagsView = "ResponseFlags.View";
    public const string ResponseFlagsCreate = "ResponseFlags.Create";
    public const string ResponseFlagsAnswer = "ResponseFlags.Answer";
    public const string ResponseFlagsEditBrief = "ResponseFlags.EditBrief";
    public const string FlagOfficersCreate = "FlagOfficers.Create";
    public const string FlagOfficersViewAll = "FlagOfficers.ViewAll";
    public const string FlagOfficersViewDetails = "FlagOfficers.ViewDetails";
    public const string FlagOfficersUpdate = "FlagOfficers.Update";
    public const string FlagOfficersDeactivate = "FlagOfficers.Deactivate";
    public const string FlagOfficersRestore = "FlagOfficers.Restore";
    public const string FlagOfficersAssignRoles = "FlagOfficers.AssignRoles";

    public const string FlagOfficerRole = "Flag Officer";
    public const string CreateOnSubmitRole = "Submit Response Flags";
    public const string ViewRole = "View Response Flags";
    public const string CreateRole = "Create Response Flags";
    public const string AnswerRole = "Answer Response Flags";
    public const string EditBriefRole = "Edit Response Flag Briefs";
}
