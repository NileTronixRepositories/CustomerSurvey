using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranchAdmin
{
    internal sealed class CreateBranchAdminCommandHandler
        : ICommandHandler<CreateBranchAdminCommand, CreateBranchAdminResponse>
    {
        private const string BranchAdministratorRoleName = "Branch Administrator";

        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<Branch> _branchReadRepository;

        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;

        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteRepository<BranchAdmin> _branchAdminWriteRepository;

        private readonly IWriteReadRepository<Role> _roleReadRepository;
        private readonly IWriteRepository<UserRole> _userRoleWriteRepository;

        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public CreateBranchAdminCommandHandler(
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteRepository<BranchAdmin> branchAdminWriteRepository,
            IWriteReadRepository<Role> roleReadRepository,
            IWriteRepository<UserRole> userRoleWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _branchReadRepository = branchReadRepository
                ?? throw new ArgumentNullException(nameof(branchReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _applicationUserWriteRepository = applicationUserWriteRepository
                ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchAdminWriteRepository = branchAdminWriteRepository
                ?? throw new ArgumentNullException(nameof(branchAdminWriteRepository));

            _roleReadRepository = roleReadRepository
                ?? throw new ArgumentNullException(nameof(roleReadRepository));

            _userRoleWriteRepository = userRoleWriteRepository
                ?? throw new ArgumentNullException(nameof(userRoleWriteRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));

            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<Result<CreateBranchAdminResponse>> Handle(
            CreateBranchAdminCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateBranchAdminResponse>.Fail(new Error(
                    Code: "BranchAdmins.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<CreateBranchAdminResponse>.Fail(new Error(
                    Code: "BranchAdmins.Create.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var branchExists = await _branchReadRepository.AnyAsync(
                x => x.Id == request.BranchId,
                cancellationToken);

            if (!branchExists)
            {
                return Result<CreateBranchAdminResponse>.Fail(new Error(
                    Code: "BranchAdmins.Create.BranchNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_Branch_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedUserName = request.UserName.Trim();
            var normalizedEmail = request.Email.Trim();

            var userNameExists = await _applicationUserReadRepository.AnyAsync(
                x => x.UserName == normalizedUserName,
                cancellationToken);

            if (userNameExists)
            {
                return Result<CreateBranchAdminResponse>.Fail(new Error(
                    Code: "BranchAdmins.Create.UserNameAlreadyExists",
                    Message: ErrorMessage.CreateBranchAdmin_UserName_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var emailExists = await _applicationUserReadRepository.AnyAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);

            if (emailExists)
            {
                return Result<CreateBranchAdminResponse>.Fail(new Error(
                    Code: "BranchAdmins.Create.EmailAlreadyExists",
                    Message: ErrorMessage.CreateBranchAdmin_Email_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var branchAdministratorRole = await _roleReadRepository.FirstOrDefaultAsync(
                new GetRoleByNameForCreateBranchAdminSpec(BranchAdministratorRoleName),
                cancellationToken);

            if (branchAdministratorRole is null)
            {
                return Result<CreateBranchAdminResponse>.Fail(new Error(
                    Code: "BranchAdmins.Create.BranchAdministratorRoleNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_BranchAdministratorRole_NotFound,
                    Type: ErrorType.NotFound));
            }

            var passwordHash = _passwordHasher.HashPassword(
     user: null!,
     password: request.Password);

            var applicationUser = ApplicationUser.Create(
                userName: request.UserName,
                passwordHash: passwordHash,
                email: request.Email,
                phoneNumber: request.PhoneNumber,
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                userType: Domain.Enums.UserType.BranchAdmin,
                createdByApplicationUserId: currentApplicationUserId);

            applicationUser.SetPasswordHash(passwordHash);

            var branchAdmin = BranchAdmin.Create(
                applicationUserId: applicationUser.Id,
                branchId: request.BranchId,
                createdByApplicationUserId: currentApplicationUserId);

            var userRole = UserRole.Create(
                applicationUserId: applicationUser.Id,
                roleId: branchAdministratorRole.Id,
                createdByApplicationUserId: currentApplicationUserId);

            await _applicationUserWriteRepository.AddAsync(applicationUser, cancellationToken);
            await _branchAdminWriteRepository.AddAsync(branchAdmin, cancellationToken);
            await _userRoleWriteRepository.AddAsync(userRole, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateBranchAdminResponse
            {
                ApplicationUserId = applicationUser.Id,
                BranchAdminId = branchAdmin.Id,
                BranchId = branchAdmin.BranchId,
                NameEn = applicationUser.NameEn,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email ?? string.Empty
            };

            return Result<CreateBranchAdminResponse>.Ok(response);
        }
    }
}