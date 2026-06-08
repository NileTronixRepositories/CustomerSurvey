using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

namespace CustomerSurvey.Application.Features.Operators.Command.RestoreOperator
{
    internal sealed class RestoreOperatorCommandHandler
        : ICommandHandler<RestoreOperatorCommand, RestoreOperatorResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreOperatorCommandHandler(
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

        public async Task<Result<RestoreOperatorResponse>> Handle(
            RestoreOperatorCommand request,
            CancellationToken cancellationToken)
        {
            var currentDepartmentAdmin = await GetCurrentDepartmentAdminAsync(cancellationToken);
            if (currentDepartmentAdmin.IsFailure)
            {
                return Result<RestoreOperatorResponse>.Fail(currentDepartmentAdmin.Errors);
            }

            var operatorProfile = await _operatorReadRepository.FirstOrDefaultAsync(
                new GetOperatorForRestoreOperatorSpec(request.OperatorId),
                cancellationToken);

            if (operatorProfile is null)
            {
                return Result<RestoreOperatorResponse>.Fail(new Error(
                    Code: "Operators.Restore.TargetNotFound",
                    Message: ErrorMessage.RestoreOperator_Operator_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (operatorProfile.DepartmentId != currentDepartmentAdmin.Value.DepartmentId)
            {
                return Result<RestoreOperatorResponse>.Fail(new Error(
                    Code: "Operators.Restore.Forbidden",
                    Message: ErrorMessage.RestoreOperator_DepartmentScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var applicationUser = await _applicationUserReadRepository.GetByIdTrackedAsync(
                operatorProfile.ApplicationUserId,
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<RestoreOperatorResponse>.Fail(new Error(
                    Code: "Operators.Restore.ApplicationUserNotFound",
                    Message: ErrorMessage.RestoreOperator_ApplicationUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (applicationUser.IsActive)
            {
                return Result<RestoreOperatorResponse>.Fail(new Error(
                    Code: "Operators.Restore.AlreadyActive",
                    Message: ErrorMessage.RestoreOperator_AlreadyActive,
                    Type: ErrorType.Validation));
            }

            applicationUser.Activate();
            _applicationUserWriteRepository.Update(applicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<RestoreOperatorResponse>.Ok(new RestoreOperatorResponse
            {
                OperatorId = operatorProfile.OperatorId,
                ApplicationUserId = applicationUser.Id,
                DepartmentId = operatorProfile.DepartmentId,
                IsActive = applicationUser.IsActive
            });
        }

        private async Task<Result<DepartmentAdminForRestoreOperatorDto>> GetCurrentDepartmentAdminAsync(
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<DepartmentAdminForRestoreOperatorDto>.Fail(new Error(
                    Code: "Operators.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentApplicationUserActive = await _applicationUserReadRepository.AnyAsync(
                x => x.Id == currentApplicationUserId && x.IsActive,
                cancellationToken);

            var departmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentDepartmentAdminForRestoreOperatorSpec(currentApplicationUserId),
                cancellationToken);

            return currentApplicationUserActive && departmentAdmin is not null
                ? Result<DepartmentAdminForRestoreOperatorDto>.Ok(departmentAdmin)
                : Result<DepartmentAdminForRestoreOperatorDto>.Fail(new Error(
                    Code: "Operators.Restore.CurrentDepartmentAdminNotFound",
                    Message: ErrorMessage.RestoreOperator_CurrentDepartmentAdmin_NotFound,
                    Type: ErrorType.NotFound));
        }
    }
}
