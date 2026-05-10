using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentsForSelection
{
    internal sealed class GetDepartmentsForSelectionQueryHandler
       : IQueryHandler<GetDepartmentsForSelectionQuery, IReadOnlyCollection<DepartmentSelectionResponse>>
    {
        private readonly IWriteReadRepository<Department> _departmentReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetDepartmentsForSelectionQueryHandler(
            IWriteReadRepository<Department> departmentReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser)
        {
            _departmentReadRepository = departmentReadRepository
                ?? throw new ArgumentNullException(nameof(departmentReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<IReadOnlyCollection<DepartmentSelectionResponse>>> Handle(
            GetDepartmentsForSelectionQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<IReadOnlyCollection<DepartmentSelectionResponse>>.Fail(new Error(
                    Code: "Departments.Selection.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<IReadOnlyCollection<DepartmentSelectionResponse>>.Fail(new Error(
                    Code: "Departments.Selection.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.GetDepartmentsSelection_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var spec = new GetDepartmentsForSelectionSpec();

            var departments = await _departmentReadRepository.ListAsync(
                spec,
                cancellationToken);

            return Result<IReadOnlyCollection<DepartmentSelectionResponse>>.Ok(departments);
        }
    }
}