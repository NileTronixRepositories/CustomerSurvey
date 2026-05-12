using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.CreateOperator
{
    using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

    internal sealed class CreateOperatorCommandHandler
       : ICommandHandler<CreateOperatorCommand, CreateOperatorResponse>
    {
        private const string OperatorRoleName = "Operator";

        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;

        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;

        private readonly IWriteReadRepository<Department> _departmentReadRepository;

        private readonly IWriteRepository<DomainOperator> _operatorWriteRepository;

        private readonly IWriteReadRepository<Role> _roleReadRepository;
        private readonly IWriteRepository<UserRole> _userRoleWriteRepository;

        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public CreateOperatorCommandHandler(
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            IWriteReadRepository<Department> departmentReadRepository,
            IWriteRepository<DomainOperator> operatorWriteRepository,
            IWriteReadRepository<Role> roleReadRepository,
            IWriteRepository<UserRole> userRoleWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _applicationUserWriteRepository = applicationUserWriteRepository
                ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _departmentAdminReadRepository = departmentAdminReadRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

            _departmentReadRepository = departmentReadRepository
                ?? throw new ArgumentNullException(nameof(departmentReadRepository));

            _operatorWriteRepository = operatorWriteRepository
                ?? throw new ArgumentNullException(nameof(operatorWriteRepository));

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

        public async Task<Result<CreateOperatorResponse>> Handle(
            CreateOperatorCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorResult = await ResolveActorAsync(
                currentApplicationUserId,
                request.DepartmentId,
                cancellationToken);

            if (actorResult.Error is not null)
            {
                return Result<CreateOperatorResponse>.Fail(actorResult.Error);
            }

            var targetDepartmentId = actorResult.DepartmentId;

            var departmentExists = await _departmentReadRepository.AnyAsync(
                x => x.Id == targetDepartmentId && x.IsActive,
                cancellationToken);

            if (!departmentExists)
            {
                return Result<CreateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Create.DepartmentNotFoundOrInactive",
                    Message: ErrorMessage.CreateOperator_Department_NotFoundOrInactive,
                    Type: ErrorType.NotFound));
            }

            var normalizedUserName = request.UserName.Trim();
            var normalizedEmail = request.Email.Trim();

            var userNameExists = await _applicationUserReadRepository.AnyAsync(
                x => x.UserName == normalizedUserName,
                cancellationToken);

            if (userNameExists)
            {
                return Result<CreateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Create.UserNameAlreadyExists",
                    Message: ErrorMessage.CreateOperator_UserName_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var emailExists = await _applicationUserReadRepository.AnyAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);

            if (emailExists)
            {
                return Result<CreateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Create.EmailAlreadyExists",
                    Message: ErrorMessage.CreateOperator_Email_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var operatorRole = await _roleReadRepository.FirstOrDefaultAsync(
                new GetOperatorRoleForCreateOperatorSpec(),
                cancellationToken);

            if (operatorRole is null)
            {
                return Result<CreateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Create.OperatorRoleNotFound",
                    Message: ErrorMessage.CreateOperator_OperatorRole_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!string.Equals(operatorRole.Name, OperatorRoleName, StringComparison.Ordinal))
            {
                return Result<CreateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Create.OperatorRoleInvalid",
                    Message: ErrorMessage.CreateOperator_OperatorRole_NotFound,
                    Type: ErrorType.NotFound));
            }

            var passwordHash = _passwordHasher.HashPassword(
                user: null!,
                password: request.Password);

            var applicationUser = ApplicationUser.Create(
                userName: normalizedUserName,
                passwordHash: passwordHash,
                email: normalizedEmail,
                phoneNumber: request.PhoneNumber,
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                userType: Domain.Enums.UserType.Operator,
                createdByApplicationUserId: currentApplicationUserId);

            var operatorProfile = Operator.Create(
                applicationUserId: applicationUser.Id,
                departmentId: targetDepartmentId,
                createdByApplicationUserId: currentApplicationUserId);

            var userRole = UserRole.Create(
                applicationUserId: applicationUser.Id,
                roleId: operatorRole.Id,
                createdByApplicationUserId: currentApplicationUserId);

            await _applicationUserWriteRepository.AddAsync(applicationUser, cancellationToken);
            await _operatorWriteRepository.AddAsync(operatorProfile, cancellationToken);
            await _userRoleWriteRepository.AddAsync(userRole, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateOperatorResponse
            {
                ApplicationUserId = applicationUser.Id,
                OperatorId = operatorProfile.Id,
                DepartmentId = operatorProfile.DepartmentId,
                NameEn = applicationUser.NameEn,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email ?? string.Empty
            };

            return Result<CreateOperatorResponse>.Ok(response);
        }

        private async Task<CreateOperatorActorResolutionResult> ResolveActorAsync(
            Guid currentApplicationUserId,
            Guid? requestedDepartmentId,
            CancellationToken cancellationToken)
        {
            var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            var currentDepartmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentDepartmentAdminForCreateOperatorSpec(currentApplicationUserId),
                cancellationToken);

            var actorMatchCount =
                (isSuperAdmin ? 1 : 0) +
                (currentDepartmentAdmin is not null ? 1 : 0);

            if (actorMatchCount == 0)
            {
                return CreateOperatorActorResolutionResult.Fail(new Error(
                    Code: "Operators.Create.CurrentActorNotAllowed",
                    Message: ErrorMessage.CreateOperator_CurrentActor_NotAllowed,
                    Type: ErrorType.Security));
            }

            if (actorMatchCount > 1)
            {
                return CreateOperatorActorResolutionResult.Fail(new Error(
                    Code: "Operators.Create.ActorAmbiguous",
                    Message: ErrorMessage.CreateOperator_Actor_Ambiguous,
                    Type: ErrorType.Security));
            }

            if (isSuperAdmin)
            {
                if (!requestedDepartmentId.HasValue || requestedDepartmentId.Value == Guid.Empty)
                {
                    return CreateOperatorActorResolutionResult.Fail(new Error(
                        Code: "Operators.Create.DepartmentIdRequiredForSuperAdmin",
                        Message: ErrorMessage.CreateOperator_DepartmentId_Required_ForSuperAdmin,
                        Type: ErrorType.Validation));
                }

                return CreateOperatorActorResolutionResult.Ok(
                    departmentId: requestedDepartmentId.Value);
            }

            if (requestedDepartmentId.HasValue &&
                requestedDepartmentId.Value != Guid.Empty &&
                requestedDepartmentId.Value != currentDepartmentAdmin!.DepartmentId)
            {
                return CreateOperatorActorResolutionResult.Fail(new Error(
                    Code: "Operators.Create.DepartmentScopeMismatch",
                    Message: ErrorMessage.CreateOperator_DepartmentScope_Mismatch,
                    Type: ErrorType.Security));
            }

            return CreateOperatorActorResolutionResult.Ok(
                departmentId: currentDepartmentAdmin!.DepartmentId);
        }

        private sealed record CreateOperatorActorResolutionResult
        {
            public Guid DepartmentId { get; init; }

            public Error? Error { get; init; }

            public static CreateOperatorActorResolutionResult Ok(Guid departmentId)
            {
                return new CreateOperatorActorResolutionResult
                {
                    DepartmentId = departmentId
                };
            }

            public static CreateOperatorActorResolutionResult Fail(Error error)
            {
                return new CreateOperatorActorResolutionResult
                {
                    Error = error
                };
            }
        }
    }
}