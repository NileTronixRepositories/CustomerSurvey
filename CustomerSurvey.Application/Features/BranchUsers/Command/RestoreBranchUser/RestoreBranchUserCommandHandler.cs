using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.RestoreBranchUser
{
    internal sealed class RestoreBranchUserCommandHandler
        : ICommandHandler<RestoreBranchUserCommand, RestoreBranchUserResponse>
    {
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreBranchUserCommandHandler(
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
        }

        public async Task<Result<RestoreBranchUserResponse>> Handle(
            RestoreBranchUserCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<RestoreBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForRestoreBranchUserSpec(currentApplicationUserId),
                cancellationToken);

            if (currentBranchAdmin is null)
            {
                return Result<RestoreBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Restore.CurrentBranchAdminNotFound",
                    Message: ErrorMessage.RestoreBranchUser_CurrentBranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var targetBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetTargetBranchUserForRestoreBranchUserSpec(request.ApplicationUserId),
                cancellationToken);

            if (targetBranchUser is null)
            {
                return Result<RestoreBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Restore.BranchUserNotFound",
                    Message: ErrorMessage.RestoreBranchUser_BranchUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (targetBranchUser.BranchId != currentBranchAdmin.BranchId)
            {
                return Result<RestoreBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Restore.BranchScopeMismatch",
                    Message: ErrorMessage.RestoreBranchUser_BranchScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var applicationUser = await _applicationUserReadRepository.FirstOrDefaultAsync(
                new GetApplicationUserForRestoreBranchUserSpec(request.ApplicationUserId),
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<RestoreBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Restore.ApplicationUserNotFound",
                    Message: ErrorMessage.RestoreBranchUser_ApplicationUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (applicationUser.IsActive)
            {
                return Result<RestoreBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Restore.AlreadyActive",
                    Message: ErrorMessage.RestoreBranchUser_Already_Active,
                    Type: ErrorType.Validation));
            }

            applicationUser.Activate();

            _applicationUserWriteRepository.Update(applicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new RestoreBranchUserResponse
            {
                BranchUserId = targetBranchUser.BranchUserId,
                ApplicationUserId = applicationUser.Id,
                BranchId = targetBranchUser.BranchId,
                IsActive = applicationUser.IsActive
            };

            return Result<RestoreBranchUserResponse>.Ok(response);
        }
    }
}