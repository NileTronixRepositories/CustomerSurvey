using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    public sealed record GetBranchDetailsResponse
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Code { get; init; } = string.Empty;

        public string? Address { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }

        public BranchDetailsSummaryResponse Summary { get; init; } = new();

        public IReadOnlyCollection<BranchDetailsBranchAdminResponse> BranchAdmins { get; init; }
            = Array.Empty<BranchDetailsBranchAdminResponse>();

        public IReadOnlyCollection<BranchDetailsBranchUserResponse> BranchUsers { get; init; }
            = Array.Empty<BranchDetailsBranchUserResponse>();

        public IReadOnlyCollection<BranchDetailsTemplateResponse> Templates { get; init; }
            = Array.Empty<BranchDetailsTemplateResponse>();

        public IReadOnlyCollection<BranchDetailsQuestionGroupResponse> QuestionGroups { get; init; }
            = Array.Empty<BranchDetailsQuestionGroupResponse>();
    }

    public sealed record BranchDetailsSummaryResponse
    {
        public int BranchAdminsCount { get; init; }

        public int BranchUsersCount { get; init; }

        public int TemplatesCount { get; init; }

        public int QuestionGroupsCount { get; init; }

        public int QuestionsCount { get; init; }
    }

    public sealed record BranchDetailsBranchAdminResponse
    {
        public Guid BranchAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }
    }

    public sealed record BranchDetailsBranchUserResponse
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public IReadOnlyCollection<BranchDetailsUserRoleResponse> Roles { get; init; }
            = Array.Empty<BranchDetailsUserRoleResponse>();
    }

    public sealed record BranchDetailsUserRoleResponse
    {
        public Guid RoleId { get; init; }

        public string Name { get; init; } = string.Empty;
    }

    public sealed record BranchDetailsTemplateResponse
    {
        public Guid TemplateId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public string Status { get; init; } = string.Empty;

        public int QuestionsCount { get; init; }
    }

    public sealed record BranchDetailsQuestionGroupResponse
    {
        public Guid GroupId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public IReadOnlyCollection<BranchDetailsQuestionResponse> Questions { get; init; }
            = Array.Empty<BranchDetailsQuestionResponse>();
    }

    public sealed record BranchDetailsQuestionResponse
    {
        public Guid QuestionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public string Type { get; init; } = string.Empty;

        public bool IsActive { get; init; }
    }
}