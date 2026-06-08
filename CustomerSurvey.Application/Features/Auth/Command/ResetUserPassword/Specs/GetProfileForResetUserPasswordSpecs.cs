using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword.Specs
{
    internal sealed record SuperAdminProfileForResetUserPasswordDto
    {
        public Guid SuperAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }
    }

    internal sealed class GetSuperAdminProfileForResetUserPasswordSpec
        : Specification<SuperAdmin, SuperAdminProfileForResetUserPasswordDto>
    {
        public GetSuperAdminProfileForResetUserPasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new SuperAdminProfileForResetUserPasswordDto
            {
                SuperAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId
            });
        }
    }

    internal sealed record BranchAdminProfileForResetUserPasswordDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetBranchAdminProfileForResetUserPasswordSpec
        : Specification<BranchAdmin, BranchAdminProfileForResetUserPasswordDto>
    {
        public GetBranchAdminProfileForResetUserPasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new BranchAdminProfileForResetUserPasswordDto
            {
                BranchAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                BranchId = x.BranchId
            });
        }
    }

    internal sealed record DepartmentAdminProfileForResetUserPasswordDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetDepartmentAdminProfileForResetUserPasswordSpec
        : Specification<DepartmentAdmin, DepartmentAdminProfileForResetUserPasswordDto>
    {
        public GetDepartmentAdminProfileForResetUserPasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new DepartmentAdminProfileForResetUserPasswordDto
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }

    internal sealed record BranchUserProfileForResetUserPasswordDto
    {
        public Guid BranchUserId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetBranchUserProfileForResetUserPasswordSpec
        : Specification<BranchUser, BranchUserProfileForResetUserPasswordDto>
    {
        public GetBranchUserProfileForResetUserPasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new BranchUserProfileForResetUserPasswordDto
            {
                BranchUserId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                BranchId = x.BranchId
            });
        }
    }

    internal sealed record OperatorProfileForResetUserPasswordDto
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetOperatorProfileForResetUserPasswordSpec
        : Specification<CustomerSurvey.Domain.Identity.Operator, OperatorProfileForResetUserPasswordDto>
    {
        public GetOperatorProfileForResetUserPasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new OperatorProfileForResetUserPasswordDto
            {
                OperatorId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}
