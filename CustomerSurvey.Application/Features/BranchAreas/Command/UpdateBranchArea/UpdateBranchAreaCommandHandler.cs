using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.BranchAreas.Shared.Specs;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.UpdateBranchArea
{
    internal sealed class UpdateBranchAreaCommandHandler
        : ICommandHandler<UpdateBranchAreaCommand, UpdateBranchAreaResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchArea> _branchAreaReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBranchAreaCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchArea> branchAreaReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _branchAreaReadRepository = branchAreaReadRepository ?? throw new ArgumentNullException(nameof(branchAreaReadRepository));
            _applicationUserReadRepository = applicationUserReadRepository ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _applicationUserWriteRepository = applicationUserWriteRepository ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<UpdateBranchAreaResponse>> Handle(UpdateBranchAreaCommand request, CancellationToken cancellationToken)
        {
            var guard = await EnsureCurrentSuperAdminAsync(cancellationToken);
            if (guard.IsFailure)
            {
                return Result<UpdateBranchAreaResponse>.Fail(guard.Errors);
            }

            var branchArea = await _branchAreaReadRepository.FirstOrDefaultAsync(
                new GetBranchAreaForManagementSpec(request.BranchAreaId),
                cancellationToken);

            if (branchArea is null)
            {
                return Result<UpdateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Update.BranchAreaNotFound",
                    Message: ErrorMessage.AssignBranchAreaBranches_BranchArea_NotFound,
                    Type: ErrorType.NotFound));
            }

            var applicationUser = await _applicationUserReadRepository.GetByIdTrackedAsync(
                branchArea.ApplicationUserId,
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<UpdateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Update.ApplicationUserNotFound",
                    Message: ErrorMessage.BranchArea_CurrentProfile_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var emailExists = await _applicationUserReadRepository.AnyAsync(
                x => x.Id != applicationUser.Id && x.Email == normalizedEmail,
                cancellationToken);

            if (emailExists)
            {
                return Result<UpdateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Update.EmailAlreadyExists",
                    Message: ErrorMessage.CreateBranchArea_Email_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            applicationUser.UpdateProfile(
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                email: normalizedEmail,
                phoneNumber: request.PhoneNumber);

            _applicationUserWriteRepository.Update(applicationUser);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<UpdateBranchAreaResponse>.Ok(new UpdateBranchAreaResponse
            {
                BranchAreaId = branchArea.BranchAreaId,
                ApplicationUserId = applicationUser.Id,
                NameEn = applicationUser.NameEn,
                NameAr = applicationUser.NameAr,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email,
                PhoneNumber = applicationUser.PhoneNumber,
                IsActive = applicationUser.IsActive
            });
        }

        private async Task<Result> EnsureCurrentSuperAdminAsync(CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result.Fail(new Error(
                    Code: "BranchAreas.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var exists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == _currentUser.UserId.Value,
                cancellationToken);

            return exists
                ? Result.Ok()
                : Result.Fail(new Error(
                    Code: "BranchAreas.Update.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
        }
    }
}
