using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorsPagination
{
    using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

    internal sealed class GetOperatorsPaginationQueryHandler
         : IQueryHandler<GetOperatorsPaginationQuery, Pagination<OperatorPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetOperatorsPaginationQueryHandler(
            IWriteReadRepository<DomainOperator> operatorReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            ICurrentUser currentUser)
        {
            _operatorReadRepository = operatorReadRepository
                ?? throw new ArgumentNullException(nameof(operatorReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _departmentAdminReadRepository = departmentAdminReadRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<OperatorPaginationItemResponse>>> Handle(
            GetOperatorsPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<OperatorPaginationItemResponse>>.Fail(new Error(
                    Code: "Operators.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var scopeResult = await ResolveScopeAsync(
                currentApplicationUserId,
                request.DepartmentId,
                cancellationToken);

            if (scopeResult.Error is not null)
            {
                return Result<Pagination<OperatorPaginationItemResponse>>.Fail(scopeResult.Error);
            }

            request.SearchText ??= string.Empty;

            var spec = new GetOperatorsPaginationSpec(
                searchParameters: request,
                departmentId: scopeResult.DepartmentId);

            var (items, totalCount) = await _operatorReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var response = new Pagination<OperatorPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<OperatorPaginationItemResponse>>.Ok(response);
        }

        private async Task<OperatorsPaginationScopeResult> ResolveScopeAsync(
            Guid currentApplicationUserId,
            Guid? requestedDepartmentId,
            CancellationToken cancellationToken)
        {
            var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            var currentDepartmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentDepartmentAdminForOperatorsPaginationSpec(currentApplicationUserId),
                cancellationToken);

            var actorMatchCount =
                (isSuperAdmin ? 1 : 0) +
                (currentDepartmentAdmin is not null ? 1 : 0);

            if (actorMatchCount == 0)
            {
                return OperatorsPaginationScopeResult.Fail(new Error(
                    Code: "Operators.Pagination.CurrentActorNotAllowed",
                    Message: ErrorMessage.GetOperatorsPagination_CurrentActor_NotAllowed,
                    Type: ErrorType.Security));
            }

            if (actorMatchCount > 1)
            {
                return OperatorsPaginationScopeResult.Fail(new Error(
                    Code: "Operators.Pagination.ActorAmbiguous",
                    Message: ErrorMessage.GetOperatorsPagination_Actor_Ambiguous,
                    Type: ErrorType.Security));
            }

            if (isSuperAdmin)
            {
                return OperatorsPaginationScopeResult.Ok(requestedDepartmentId);
            }

            if (requestedDepartmentId.HasValue &&
                requestedDepartmentId.Value != Guid.Empty &&
                requestedDepartmentId.Value != currentDepartmentAdmin!.DepartmentId)
            {
                return OperatorsPaginationScopeResult.Fail(new Error(
                    Code: "Operators.Pagination.DepartmentScopeMismatch",
                    Message: ErrorMessage.GetOperatorsPagination_DepartmentScope_Mismatch,
                    Type: ErrorType.Security));
            }

            return OperatorsPaginationScopeResult.Ok(currentDepartmentAdmin!.DepartmentId);
        }

        private sealed record OperatorsPaginationScopeResult
        {
            public Guid? DepartmentId { get; init; }

            public Error? Error { get; init; }

            public static OperatorsPaginationScopeResult Ok(Guid? departmentId)
            {
                return new OperatorsPaginationScopeResult
                {
                    DepartmentId = departmentId
                };
            }

            public static OperatorsPaginationScopeResult Fail(Error error)
            {
                return new OperatorsPaginationScopeResult
                {
                    Error = error
                };
            }
        }
    }
}