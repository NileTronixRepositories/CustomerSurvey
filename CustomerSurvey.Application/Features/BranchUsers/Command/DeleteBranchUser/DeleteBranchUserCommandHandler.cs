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

namespace CustomerSurvey.Application.Features.BranchUsers.Command.DeleteBranchUser
{
    internal sealed class DeleteBranchUserCommandHandler
       : ICommandHandler<DeleteBranchUserCommand, DeleteBranchUserResponse>
    {
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBranchUserCommandHandler(
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

        public async Task<Result<DeleteBranchUserResponse>> Handle(
            DeleteBranchUserCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<DeleteBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Delete.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForDeleteBranchUserSpec(currentApplicationUserId),
                cancellationToken);

            if (currentBranchAdmin is null)
            {
                return Result<DeleteBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Delete.CurrentBranchAdminNotFound",
                    Message: ErrorMessage.DeleteBranchUser_CurrentBranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var targetBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetTargetBranchUserForDeleteBranchUserSpec(request.ApplicationUserId),
                cancellationToken);

            if (targetBranchUser is null)
            {
                return Result<DeleteBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Delete.BranchUserNotFound",
                    Message: ErrorMessage.DeleteBranchUser_BranchUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (targetBranchUser.BranchId != currentBranchAdmin.BranchId)
            {
                return Result<DeleteBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Delete.BranchScopeMismatch",
                    Message: ErrorMessage.DeleteBranchUser_BranchScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var applicationUser = await _applicationUserReadRepository.FirstOrDefaultAsync(
                new GetApplicationUserForDeleteBranchUserSpec(request.ApplicationUserId),
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<DeleteBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Delete.ApplicationUserNotFound",
                    Message: ErrorMessage.DeleteBranchUser_ApplicationUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!applicationUser.IsActive)
            {
                return Result<DeleteBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Delete.AlreadyInactive",
                    Message: ErrorMessage.DeleteBranchUser_Already_Inactive,
                    Type: ErrorType.Validation));
            }

            applicationUser.Deactivate();

            _applicationUserWriteRepository.Update(applicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<DeleteBranchUserResponse>.Ok(new DeleteBranchUserResponse
            {
                BranchUserId = targetBranchUser.BranchUserId,
                ApplicationUserId = applicationUser.Id,
                BranchId = targetBranchUser.BranchId,
                IsActive = applicationUser.IsActive
            });
        }
    }
}