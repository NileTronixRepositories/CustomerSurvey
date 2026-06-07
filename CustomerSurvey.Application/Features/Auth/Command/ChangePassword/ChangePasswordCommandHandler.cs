using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Auth.Command.ChangePassword.Specs;
using CustomerSurvey.Application.Options;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CustomerSurvey.Application.Features.Auth.Command.ChangePassword
{
    internal sealed class ChangePasswordCommandHandler
        : ICommandHandler<ChangePasswordCommand, ChangePasswordResponse>
    {
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordPolicyOptions _passwordPolicyOptions;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public ChangePasswordCommandHandler(
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork,
            IOptions<PasswordPolicyOptions> passwordPolicyOptions)
        {
            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordPolicyOptions = passwordPolicyOptions?.Value
                ?? throw new ArgumentNullException(nameof(passwordPolicyOptions));

            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<Result<ChangePasswordResponse>> Handle(
            ChangePasswordCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<ChangePasswordResponse>.Fail(new Error(
                    Code: "Auth.TokenMissing",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            if (request.ApplicationUserId != _currentUser.UserId.Value)
            {
                return Result<ChangePasswordResponse>.Fail(new Error(
                    Code: "ChangePassword.UserIdMismatch",
                    Message: ErrorMessage.ChangePassword_UserIdMismatch,
                    Type: ErrorType.Security));
            }

            var applicationUser = await _applicationUserReadRepository.FirstOrDefaultAsync(
                new GetApplicationUserForChangePasswordSpec(request.ApplicationUserId),
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<ChangePasswordResponse>.Fail(new Error(
                    Code: "ChangePassword.UserNotFound",
                    Message: ErrorMessage.ChangePassword_UserNotFound,
                    Type: ErrorType.NotFound));
            }

            if (!applicationUser.IsActive)
            {
                return Result<ChangePasswordResponse>.Fail(new Error(
                    Code: "ChangePassword.UserInactive",
                    Message: ErrorMessage.ChangePassword_UserInactive,
                    Type: ErrorType.Validation));
            }

            var passwordHash = _passwordHasher.HashPassword(
                applicationUser,
                request.NewPassword);

            applicationUser.ChangePassword(passwordHash);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var passwordExpiresOnUtc = applicationUser.PasswordChangedOnUtc
                .AddDays(_passwordPolicyOptions.ExpiryDays);

            return Result<ChangePasswordResponse>.Ok(new ChangePasswordResponse
            {
                ApplicationUserId = applicationUser.Id,
                PasswordChanged = true,
                FirstLoginFlag = applicationUser.IsFirstLogin,
                PasswordChangedOnUtc = applicationUser.PasswordChangedOnUtc,
                PasswordExpiredFlag = passwordExpiresOnUtc <= DateTime.UtcNow
            });
        }
    }
}
