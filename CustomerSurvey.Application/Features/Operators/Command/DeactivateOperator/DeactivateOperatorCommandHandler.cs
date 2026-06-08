using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

namespace CustomerSurvey.Application.Features.Operators.Command.DeactivateOperator
{
    internal sealed class DeactivateOperatorCommandHandler
        : ICommandHandler<DeactivateOperatorCommand, DeactivateOperatorResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateOperatorCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            IWriteReadRepository<DomainOperator> operatorReadRepository,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _applicationUserReadRepository = applicationUserReadRepository ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _applicationUserWriteRepository = applicationUserWriteRepository ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
            _departmentAdminReadRepository = departmentAdminReadRepository ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));
            _operatorReadRepository = operatorReadRepository ?? throw new ArgumentNullException(nameof(operatorReadRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<DeactivateOperatorResponse>> Handle(
            DeactivateOperatorCommand request,
            CancellationToken cancellationToken)
        {
            var currentDepartmentAdmin = await GetCurrentDepartmentAdminAsync(cancellationToken);
            if (currentDepartmentAdmin.IsFailure)
            {
                return Result<DeactivateOperatorResponse>.Fail(currentDepartmentAdmin.Errors);
            }

            var operatorProfile = await _operatorReadRepository.FirstOrDefaultAsync(
                new GetOperatorForDeactivateOperatorSpec(request.OperatorId),
                cancellationToken);

            if (operatorProfile is null)
            {
                return Result<DeactivateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Deactivate.TargetNotFound",
                    Message: ErrorMessage.DeactivateOperator_Operator_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (operatorProfile.DepartmentId != currentDepartmentAdmin.Value.DepartmentId)
            {
                return Result<DeactivateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Deactivate.Forbidden",
                    Message: ErrorMessage.DeactivateOperator_DepartmentScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var applicationUser = await _applicationUserReadRepository.GetByIdTrackedAsync(
                operatorProfile.ApplicationUserId,
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<DeactivateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Deactivate.ApplicationUserNotFound",
                    Message: ErrorMessage.DeactivateOperator_ApplicationUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!applicationUser.IsActive)
            {
                return Result<DeactivateOperatorResponse>.Fail(new Error(
                    Code: "Operators.Deactivate.AlreadyInactive",
                    Message: ErrorMessage.DeactivateOperator_AlreadyInactive,
                    Type: ErrorType.Validation));
            }

            applicationUser.Deactivate();
            _applicationUserWriteRepository.Update(applicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<DeactivateOperatorResponse>.Ok(new DeactivateOperatorResponse
            {
                OperatorId = operatorProfile.OperatorId,
                ApplicationUserId = applicationUser.Id,
                DepartmentId = operatorProfile.DepartmentId,
                IsActive = applicationUser.IsActive
            });
        }

        private async Task<Result<DepartmentAdminForDeactivateOperatorDto>> GetCurrentDepartmentAdminAsync(
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<DepartmentAdminForDeactivateOperatorDto>.Fail(new Error(
                    Code: "Operators.Deactivate.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentApplicationUserActive = await _applicationUserReadRepository.AnyAsync(
                x => x.Id == currentApplicationUserId && x.IsActive,
                cancellationToken);

            var departmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentDepartmentAdminForDeactivateOperatorSpec(currentApplicationUserId),
                cancellationToken);

            return currentApplicationUserActive && departmentAdmin is not null
                ? Result<DepartmentAdminForDeactivateOperatorDto>.Ok(departmentAdmin)
                : Result<DepartmentAdminForDeactivateOperatorDto>.Fail(new Error(
                    Code: "Operators.Deactivate.CurrentDepartmentAdminNotFound",
                    Message: ErrorMessage.DeactivateOperator_CurrentDepartmentAdmin_NotFound,
                    Type: ErrorType.NotFound));
        }
    }
}
