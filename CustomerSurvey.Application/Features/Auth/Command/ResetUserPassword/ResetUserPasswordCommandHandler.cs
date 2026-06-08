using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword.Specs;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Identity;
using DomainUserType = CustomerSurvey.Domain.Enums.UserType;

namespace CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword
{
    using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

    internal sealed class ResetUserPasswordCommandHandler
        : ICommandHandler<ResetUserPasswordCommand, ResetUserPasswordResponse>
    {
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public ResetUserPasswordCommandHandler(
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<DomainOperator> operatorReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _applicationUserWriteRepository = applicationUserWriteRepository
                ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));
            _departmentAdminReadRepository = departmentAdminReadRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));
            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));
            _operatorReadRepository = operatorReadRepository
                ?? throw new ArgumentNullException(nameof(operatorReadRepository));
            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));

            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<Result<ResetUserPasswordResponse>> Handle(
            ResetUserPasswordCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<ResetUserPasswordResponse>.Fail(new Error(
                    Code: "Users.ResetPassword.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorProfile = await ResolveActorProfileAsync(
                currentApplicationUserId,
                cancellationToken);

            if (actorProfile is null)
            {
                return Result<ResetUserPasswordResponse>.Fail(new Error(
                    Code: "Users.ResetPassword.CurrentActorNotFound",
                    Message: ErrorMessage.ResetUserPassword_CurrentActor_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (request.ApplicationUserId == currentApplicationUserId)
            {
                return Result<ResetUserPasswordResponse>.Fail(new Error(
                    Code: "Users.ResetPassword.SelfResetNotAllowed",
                    Message: ErrorMessage.ResetUserPassword_SelfReset_NotAllowed,
                    Type: ErrorType.Security));
            }

            var targetApplicationUser = await _applicationUserReadRepository.FirstOrDefaultAsync(
                new GetApplicationUserForResetUserPasswordSpec(request.ApplicationUserId),
                cancellationToken);

            if (targetApplicationUser is null)
            {
                return Result<ResetUserPasswordResponse>.Fail(new Error(
                    Code: "Users.ResetPassword.TargetUserNotFound",
                    Message: ErrorMessage.ResetUserPassword_TargetUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!targetApplicationUser.IsActive)
            {
                return Result<ResetUserPasswordResponse>.Fail(new Error(
                    Code: "Users.ResetPassword.TargetUserInactive",
                    Message: ErrorMessage.ResetUserPassword_TargetUser_Inactive,
                    Type: ErrorType.Validation));
            }

            var targetProfileResult = await ResolveTargetProfileAsync(
                targetApplicationUser,
                cancellationToken);

            if (targetProfileResult.IsFailure)
            {
                return Result<ResetUserPasswordResponse>.Fail(targetProfileResult.Errors);
            }

            var scopeError = ValidateActorCanResetTarget(
                actorProfile,
                targetProfileResult.Value);

            if (scopeError is not null)
            {
                return Result<ResetUserPasswordResponse>.Fail(scopeError);
            }

            var passwordHash = _passwordHasher.HashPassword(
                targetApplicationUser,
                request.NewPassword);

            targetApplicationUser.ResetPassword(passwordHash);

            _applicationUserWriteRepository.Update(targetApplicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<ResetUserPasswordResponse>.Ok(new ResetUserPasswordResponse
            {
                ApplicationUserId = targetApplicationUser.Id,
                PasswordChanged = true,
                MustChangePasswordOnNextLogin = targetApplicationUser.IsFirstLogin,
                PasswordChangedOnUtc = targetApplicationUser.PasswordChangedOnUtc
            });
        }

        private async Task<ActorProfile?> ResolveActorProfileAsync(
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            var superAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetSuperAdminProfileForResetUserPasswordSpec(currentApplicationUserId),
                cancellationToken);

            if (superAdmin is not null)
            {
                return ActorProfile.SuperAdmin();
            }

            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetBranchAdminProfileForResetUserPasswordSpec(currentApplicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return ActorProfile.BranchAdmin(branchAdmin.BranchId);
            }

            var departmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetDepartmentAdminProfileForResetUserPasswordSpec(currentApplicationUserId),
                cancellationToken);

            if (departmentAdmin is not null)
            {
                return ActorProfile.DepartmentAdmin(departmentAdmin.DepartmentId);
            }

            return null;
        }

        private async Task<Result<TargetProfile>> ResolveTargetProfileAsync(
            ApplicationUser targetApplicationUser,
            CancellationToken cancellationToken)
        {
            switch (targetApplicationUser.UserType)
            {
                case DomainUserType.BranchAdmin:
                    var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                        new GetBranchAdminProfileForResetUserPasswordSpec(targetApplicationUser.Id),
                        cancellationToken);

                    return branchAdmin is null
                        ? UnsupportedTargetProfile()
                        : Result<TargetProfile>.Ok(TargetProfile.BranchAdmin());

                case DomainUserType.DepartmentAdmin:
                    var departmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                        new GetDepartmentAdminProfileForResetUserPasswordSpec(targetApplicationUser.Id),
                        cancellationToken);

                    return departmentAdmin is null
                        ? UnsupportedTargetProfile()
                        : Result<TargetProfile>.Ok(TargetProfile.DepartmentAdmin());

                case DomainUserType.Operator:
                    var operatorProfile = await _operatorReadRepository.FirstOrDefaultAsync(
                        new GetOperatorProfileForResetUserPasswordSpec(targetApplicationUser.Id),
                        cancellationToken);

                    return operatorProfile is null
                        ? UnsupportedTargetProfile()
                        : Result<TargetProfile>.Ok(TargetProfile.Operator(operatorProfile.DepartmentId));

                case DomainUserType.BranchUser:
                    var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                        new GetBranchUserProfileForResetUserPasswordSpec(targetApplicationUser.Id),
                        cancellationToken);

                    return branchUser is null
                        ? UnsupportedTargetProfile()
                        : Result<TargetProfile>.Ok(TargetProfile.BranchUser(branchUser.BranchId));

                case DomainUserType.SuperAdmin:
                    return Result<TargetProfile>.Ok(TargetProfile.SuperAdmin());

                default:
                    return UnsupportedTargetProfile();
            }
        }

        private static Error? ValidateActorCanResetTarget(
            ActorProfile actorProfile,
            TargetProfile targetProfile)
        {
            if (actorProfile.Type == ActorType.SuperAdmin)
            {
                return targetProfile.Type is TargetType.BranchAdmin
                    or TargetType.DepartmentAdmin
                    or TargetType.Operator
                    or TargetType.BranchUser
                    ? null
                    : ForbiddenTargetType();
            }

            if (actorProfile.Type == ActorType.BranchAdmin)
            {
                if (targetProfile.Type != TargetType.BranchUser)
                {
                    return ForbiddenTargetType();
                }

                return targetProfile.BranchId == actorProfile.BranchId
                    ? null
                    : ScopeViolation();
            }

            if (actorProfile.Type == ActorType.DepartmentAdmin)
            {
                if (targetProfile.Type != TargetType.Operator)
                {
                    return ForbiddenTargetType();
                }

                return targetProfile.DepartmentId == actorProfile.DepartmentId
                    ? null
                    : ScopeViolation();
            }

            return ForbiddenTargetType();
        }

        private static Result<TargetProfile> UnsupportedTargetProfile()
        {
            return Result<TargetProfile>.Fail(new Error(
                Code: "Users.ResetPassword.TargetProfileNotSupported",
                Message: ErrorMessage.ResetUserPassword_TargetProfile_NotSupported,
                Type: ErrorType.Security));
        }

        private static Error ForbiddenTargetType()
        {
            return new Error(
                Code: "Users.ResetPassword.ForbiddenTargetType",
                Message: ErrorMessage.ResetUserPassword_Forbidden_TargetType,
                Type: ErrorType.Security);
        }

        private static Error ScopeViolation()
        {
            return new Error(
                Code: "Users.ResetPassword.ScopeViolation",
                Message: ErrorMessage.ResetUserPassword_ScopeViolation,
                Type: ErrorType.Security);
        }

        private enum ActorType
        {
            SuperAdmin,
            BranchAdmin,
            DepartmentAdmin
        }

        private enum TargetType
        {
            SuperAdmin,
            BranchAdmin,
            DepartmentAdmin,
            Operator,
            BranchUser
        }

        private sealed record ActorProfile(
            ActorType Type,
            Guid? BranchId = null,
            Guid? DepartmentId = null)
        {
            public static ActorProfile SuperAdmin()
                => new(ActorType.SuperAdmin);

            public static ActorProfile BranchAdmin(Guid branchId)
                => new(ActorType.BranchAdmin, BranchId: branchId);

            public static ActorProfile DepartmentAdmin(Guid departmentId)
                => new(ActorType.DepartmentAdmin, DepartmentId: departmentId);
        }

        private sealed record TargetProfile(
            TargetType Type,
            Guid? BranchId = null,
            Guid? DepartmentId = null)
        {
            public static TargetProfile SuperAdmin()
                => new(TargetType.SuperAdmin);

            public static TargetProfile BranchAdmin()
                => new(TargetType.BranchAdmin);

            public static TargetProfile DepartmentAdmin()
                => new(TargetType.DepartmentAdmin);

            public static TargetProfile Operator(Guid departmentId)
                => new(TargetType.Operator, DepartmentId: departmentId);

            public static TargetProfile BranchUser(Guid branchId)
                => new(TargetType.BranchUser, BranchId: branchId);
        }
    }
}
