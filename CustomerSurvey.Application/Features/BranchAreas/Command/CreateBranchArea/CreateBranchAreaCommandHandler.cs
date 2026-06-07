using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.BranchAreas.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Identity;
using CustomerUserType = CustomerSurvey.Domain.Enums.UserType;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.CreateBranchArea
{
    internal sealed class CreateBranchAreaCommandHandler
        : ICommandHandler<CreateBranchAreaCommand, CreateBranchAreaResponse>
    {
        private const string BranchAreaAdministratorRoleName = "Branch Area Administrator";

        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IWriteRepository<BranchArea> _branchAreaWriteRepository;
        private readonly IWriteRepository<BranchAreaBranch> _branchAreaBranchWriteRepository;
        private readonly IWriteReadRepository<Role> _roleReadRepository;
        private readonly IWriteRepository<UserRole> _userRoleWriteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public CreateBranchAreaCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteRepository<BranchArea> branchAreaWriteRepository,
            IWriteRepository<BranchAreaBranch> branchAreaBranchWriteRepository,
            IWriteReadRepository<Role> roleReadRepository,
            IWriteRepository<UserRole> userRoleWriteRepository,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _branchReadRepository = branchReadRepository ?? throw new ArgumentNullException(nameof(branchReadRepository));
            _applicationUserReadRepository = applicationUserReadRepository ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _applicationUserWriteRepository = applicationUserWriteRepository ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
            _branchAreaWriteRepository = branchAreaWriteRepository ?? throw new ArgumentNullException(nameof(branchAreaWriteRepository));
            _branchAreaBranchWriteRepository = branchAreaBranchWriteRepository ?? throw new ArgumentNullException(nameof(branchAreaBranchWriteRepository));
            _roleReadRepository = roleReadRepository ?? throw new ArgumentNullException(nameof(roleReadRepository));
            _userRoleWriteRepository = userRoleWriteRepository ?? throw new ArgumentNullException(nameof(userRoleWriteRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<Result<CreateBranchAreaResponse>> Handle(
            CreateBranchAreaCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<CreateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Create.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var requestedBranchIds = request.BranchIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToArray();

            if (requestedBranchIds.Length == 0)
            {
                return Result<CreateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Create.BranchesRequired",
                    Message: ErrorMessage.CreateBranchArea_Branches_Required,
                    Type: ErrorType.Validation));
            }

            if (requestedBranchIds.Length != request.BranchIds.Count)
            {
                return Result<CreateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Create.BranchIdsDuplicatedOrInvalid",
                    Message: request.BranchIds.Any(x => x == Guid.Empty)
                        ? ErrorMessage.CreateBranchArea_BranchIds_Invalid
                        : ErrorMessage.CreateBranchArea_BranchIds_Duplicated,
                    Type: ErrorType.Validation));
            }

            var branches = await _branchReadRepository.ListAsync(
                new GetActiveBranchesForBranchAreaAssignmentSpec(requestedBranchIds),
                cancellationToken);

            if (branches.Count != requestedBranchIds.Length)
            {
                return Result<CreateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Create.BranchesNotFound",
                    Message: ErrorMessage.CreateBranchArea_Branches_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedUserName = request.UserName.Trim();
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var userNameExists = await _applicationUserReadRepository.AnyAsync(
                x => x.UserName == normalizedUserName,
                cancellationToken);

            if (userNameExists)
            {
                return Result<CreateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Create.UserNameAlreadyExists",
                    Message: ErrorMessage.CreateBranchArea_UserName_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var emailExists = await _applicationUserReadRepository.AnyAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);

            if (emailExists)
            {
                return Result<CreateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Create.EmailAlreadyExists",
                    Message: ErrorMessage.CreateBranchArea_Email_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var role = await _roleReadRepository.FirstOrDefaultAsync(
                new GetRoleByNameForCreateBranchAreaSpec(BranchAreaAdministratorRoleName),
                cancellationToken);

            if (role is null)
            {
                return Result<CreateBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Create.BranchAreaRoleNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_BranchAdministratorRole_NotFound,
                    Type: ErrorType.NotFound));
            }

            var passwordHash = _passwordHasher.HashPassword(null!, request.Password);

            var applicationUser = ApplicationUser.Create(
                userName: normalizedUserName,
                email: normalizedEmail,
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                phoneNumber: request.PhoneNumber,
                passwordHash: passwordHash,
                userType: CustomerUserType.BranchArea,
                createdByApplicationUserId: currentApplicationUserId);

            var branchArea = BranchArea.Create(
                applicationUserId: applicationUser.Id,
                createdByApplicationUserId: currentApplicationUserId);

            var userRole = UserRole.Create(
                applicationUserId: applicationUser.Id,
                roleId: role.Id,
                createdByApplicationUserId: currentApplicationUserId);

            var assignments = requestedBranchIds
                .Select(branchId => BranchAreaBranch.Create(
                    branchAreaId: branchArea.Id,
                    branchId: branchId,
                    createdByApplicationUserId: currentApplicationUserId))
                .ToList();

            await _applicationUserWriteRepository.AddAsync(applicationUser, cancellationToken);
            await _branchAreaWriteRepository.AddAsync(branchArea, cancellationToken);
            await _userRoleWriteRepository.AddAsync(userRole, cancellationToken);
            await _branchAreaBranchWriteRepository.AddRangeAsync(assignments, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreateBranchAreaResponse>.Ok(new CreateBranchAreaResponse
            {
                BranchAreaId = branchArea.Id,
                ApplicationUserId = applicationUser.Id,
                NameEn = applicationUser.NameEn,
                NameAr = applicationUser.NameAr,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email,
                PhoneNumber = applicationUser.PhoneNumber,
                Branches = branches
            });
        }
    }
}
