using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.ResetBranchUserPassword
{
    internal sealed class ResetBranchUserPasswordCommandHandler
        : ICommandHandler<ResetBranchUserPasswordCommand, ResetBranchUserPasswordResponse>
    {
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public ResetBranchUserPasswordCommandHandler(
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));
            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));
            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _applicationUserWriteRepository = applicationUserWriteRepository
                ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));

            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<Result<ResetBranchUserPasswordResponse>> Handle(
            ResetBranchUserPasswordCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<ResetBranchUserPasswordResponse>.Fail(new Error(
                    Code: "BranchUsers.ResetPassword.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForResetBranchUserPasswordSpec(currentApplicationUserId),
                cancellationToken);

            if (currentBranchAdmin is null)
            {
                return Result<ResetBranchUserPasswordResponse>.Fail(new Error(
                    Code: "BranchUsers.ResetPassword.CurrentBranchAdminNotFound",
                    Message: ErrorMessage.ResetBranchUserPassword_CurrentBranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var targetBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetTargetBranchUserForResetBranchUserPasswordSpec(request.ApplicationUserId),
                cancellationToken);

            if (targetBranchUser is null)
            {
                return Result<ResetBranchUserPasswordResponse>.Fail(new Error(
                    Code: "BranchUsers.ResetPassword.BranchUserNotFound",
                    Message: ErrorMessage.ResetBranchUserPassword_BranchUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (targetBranchUser.BranchId != currentBranchAdmin.BranchId)
            {
                return Result<ResetBranchUserPasswordResponse>.Fail(new Error(
                    Code: "BranchUsers.ResetPassword.BranchScopeMismatch",
                    Message: ErrorMessage.ResetBranchUserPassword_BranchScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var applicationUser = await _applicationUserReadRepository.FirstOrDefaultAsync(
                new GetApplicationUserForResetBranchUserPasswordSpec(request.ApplicationUserId),
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<ResetBranchUserPasswordResponse>.Fail(new Error(
                    Code: "BranchUsers.ResetPassword.ApplicationUserNotFound",
                    Message: ErrorMessage.ResetBranchUserPassword_ApplicationUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!applicationUser.IsActive)
            {
                return Result<ResetBranchUserPasswordResponse>.Fail(new Error(
                    Code: "BranchUsers.ResetPassword.UserInactive",
                    Message: ErrorMessage.ResetBranchUserPassword_User_Inactive,
                    Type: ErrorType.Validation));
            }

            var passwordHash = _passwordHasher.HashPassword(
                applicationUser,
                request.NewPassword);

            applicationUser.SetPasswordHash(passwordHash);

            _applicationUserWriteRepository.Update(applicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<ResetBranchUserPasswordResponse>.Ok(new ResetBranchUserPasswordResponse
            {
                BranchUserId = targetBranchUser.BranchUserId,
                ApplicationUserId = applicationUser.Id,
                BranchId = targetBranchUser.BranchId,
                PasswordReset = true
            });
        }
    }
}