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

namespace CustomerSurvey.Application.Features.BranchUsers.Command.UpdateBranchUser
{
    internal sealed class UpdateBranchUserCommandHandler
      : ICommandHandler<UpdateBranchUserCommand, UpdateBranchUserResponse>
    {
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBranchUserCommandHandler(
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

        public async Task<Result<UpdateBranchUserResponse>> Handle(
            UpdateBranchUserCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForUpdateBranchUserSpec(currentApplicationUserId),
                cancellationToken);

            if (currentBranchAdmin is null)
            {
                return Result<UpdateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Update.CurrentBranchAdminNotFound",
                    Message: ErrorMessage.UpdateBranchUser_CurrentBranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var targetBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetTargetBranchUserForUpdateBranchUserSpec(request.ApplicationUserId),
                cancellationToken);

            if (targetBranchUser is null)
            {
                return Result<UpdateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Update.BranchUserNotFound",
                    Message: ErrorMessage.UpdateBranchUser_BranchUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (targetBranchUser.BranchId != currentBranchAdmin.BranchId)
            {
                return Result<UpdateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Update.BranchScopeMismatch",
                    Message: ErrorMessage.UpdateBranchUser_BranchScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var applicationUser = await _applicationUserReadRepository.FirstOrDefaultAsync(
                new GetApplicationUserForUpdateBranchUserSpec(request.ApplicationUserId),
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<UpdateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Update.ApplicationUserNotFound",
                    Message: ErrorMessage.UpdateBranchUser_ApplicationUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!applicationUser.IsActive)
            {
                return Result<UpdateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Update.UserInactive",
                    Message: ErrorMessage.UpdateBranchUser_User_Inactive,
                    Type: ErrorType.Validation));
            }

            var normalizedEmail = request.Email.Trim();

            //var userNameExists = await _applicationUserReadRepository.AnyAsync(
            //    x => x.Id != request.ApplicationUserId && x.UserName == normalizedUserName,
            //    cancellationToken);

            //if (userNameExists)
            //{
            //    return Result<UpdateBranchUserResponse>.Fail(new Error(
            //        Code: "BranchUsers.Update.UserNameAlreadyExists",
            //        Message: ErrorMessage.UpdateBranchUser_UserName_AlreadyExists,
            //        Type: ErrorType.Validation));
            //}

            var emailExists = await _applicationUserReadRepository.AnyAsync(
                x => x.Id != request.ApplicationUserId && x.Email == normalizedEmail,
                cancellationToken);

            if (emailExists)
            {
                return Result<UpdateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Update.EmailAlreadyExists",
                    Message: ErrorMessage.UpdateBranchUser_Email_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            applicationUser.UpdateProfile(
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                email: normalizedEmail,
                phoneNumber: request.PhoneNumber);

            _applicationUserWriteRepository.Update(applicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<UpdateBranchUserResponse>.Ok(new UpdateBranchUserResponse
            {
                BranchUserId = targetBranchUser.BranchUserId,
                ApplicationUserId = applicationUser.Id,
                BranchId = targetBranchUser.BranchId,
                NameEn = applicationUser.NameEn,
                NameAr = applicationUser.NameAr,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email ?? string.Empty,
                PhoneNumber = applicationUser.PhoneNumber,
                IsActive = applicationUser.IsActive
            });
        }
    }
}