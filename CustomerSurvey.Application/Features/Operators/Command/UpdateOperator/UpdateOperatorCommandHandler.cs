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

namespace CustomerSurvey.Application.Features.Operators.Command.UpdateOperator
{
    internal sealed class UpdateOperatorCommandHandler
        : ICommandHandler<UpdateOperatorCommand, UpdateOperatorResponse>
    {
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;

        private readonly IWriteReadRepository<Domain.Identity.Operator> _operatorReadRepository;

        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;

        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOperatorCommandHandler(
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            IWriteReadRepository<Domain.Identity.Operator> operatorReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _departmentAdminReadRepository = departmentAdminReadRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

            _operatorReadRepository = operatorReadRepository
                ?? throw new ArgumentNullException(nameof(operatorReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _applicationUserWriteRepository = applicationUserWriteRepository
                ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<UpdateOperatorResponse>> Handle(
            UpdateOperatorCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorResult = await ResolveActorAsync(
                currentApplicationUserId,
                cancellationToken);

            if (actorResult.Error is not null)
            {
                return Result<UpdateOperatorResponse>.Fail(actorResult.Error);
            }

            var operatorProfile = await _operatorReadRepository.FirstOrDefaultAsync(
                new GetOperatorForUpdateOperatorSpec(request.OperatorId),
                cancellationToken);

            if (operatorProfile is null)
            {
                return Result<UpdateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Update.OperatorNotFound",
                    Message: ErrorMessage.UpdateOperator_Operator_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (actorResult.DepartmentId.HasValue &&
                operatorProfile.DepartmentId != actorResult.DepartmentId.Value)
            {
                return Result<UpdateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Update.DepartmentScopeMismatch",
                    Message: ErrorMessage.UpdateOperator_DepartmentScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var applicationUser = await _applicationUserReadRepository.FirstOrDefaultAsync(
                new GetApplicationUserForUpdateOperatorSpec(operatorProfile.ApplicationUserId),
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<UpdateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Update.ApplicationUserNotFound",
                    Message: ErrorMessage.UpdateOperator_ApplicationUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedEmail = request.Email.Trim();

            var emailExists = await _applicationUserReadRepository.AnyAsync(
                x => x.Email == normalizedEmail && x.Id != applicationUser.Id,
                cancellationToken);

            if (emailExists)
            {
                return Result<UpdateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Update.EmailAlreadyExists",
                    Message: ErrorMessage.UpdateOperator_Email_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            applicationUser.UpdateProfile(
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                email: normalizedEmail,
                phoneNumber: request.PhoneNumber);

            _applicationUserWriteRepository.Update(applicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UpdateOperatorResponse
            {
                OperatorId = operatorProfile.OperatorId,
                ApplicationUserId = applicationUser.Id,
                DepartmentId = operatorProfile.DepartmentId,
                NameEn = applicationUser.NameEn,
                NameAr = applicationUser.NameAr,
                Email = applicationUser.Email ?? string.Empty,
                PhoneNumber = applicationUser.PhoneNumber
            };

            return Result<UpdateOperatorResponse>.Ok(response);
        }

        private async Task<UpdateOperatorActorResolutionResult> ResolveActorAsync(
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            var departmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentDepartmentAdminForUpdateOperatorSpec(currentApplicationUserId),
                cancellationToken);

            var actorMatchCount =
                (isSuperAdmin ? 1 : 0) +
                (departmentAdmin is not null ? 1 : 0);

            if (actorMatchCount == 0)
            {
                return UpdateOperatorActorResolutionResult.Fail(new Error(
                    Code: "Operators.Update.CurrentActorNotAllowed",
                    Message: ErrorMessage.UpdateOperator_CurrentActor_NotAllowed,
                    Type: ErrorType.Security));
            }

            if (actorMatchCount > 1)
            {
                return UpdateOperatorActorResolutionResult.Fail(new Error(
                    Code: "Operators.Update.ActorAmbiguous",
                    Message: ErrorMessage.UpdateOperator_Actor_Ambiguous,
                    Type: ErrorType.Security));
            }

            if (isSuperAdmin)
            {
                return UpdateOperatorActorResolutionResult.Ok(departmentId: null);
            }

            return UpdateOperatorActorResolutionResult.Ok(
                departmentId: departmentAdmin!.DepartmentId);
        }

        private sealed record UpdateOperatorActorResolutionResult
        {
            public Guid? DepartmentId { get; init; }

            public Error? Error { get; init; }

            public static UpdateOperatorActorResolutionResult Ok(Guid? departmentId)
            {
                return new UpdateOperatorActorResolutionResult
                {
                    DepartmentId = departmentId
                };
            }

            public static UpdateOperatorActorResolutionResult Fail(Error error)
            {
                return new UpdateOperatorActorResolutionResult
                {
                    Error = error
                };
            }
        }
    }
}