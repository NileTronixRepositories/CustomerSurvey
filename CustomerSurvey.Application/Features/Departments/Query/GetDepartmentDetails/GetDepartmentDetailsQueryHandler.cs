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

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentDetails
{
    internal sealed class GetDepartmentDetailsQueryHandler
           : IQueryHandler<GetDepartmentDetailsQuery, GetDepartmentDetailsResponse>
    {
        private readonly IWriteReadRepository<Department> _departmentReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly IWriteReadRepository<Operator> _operatorReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetDepartmentDetailsQueryHandler(
            IWriteReadRepository<Department> departmentReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            IWriteReadRepository<Operator> operatorReadRepository,
            ICurrentUser currentUser)
        {
            _departmentReadRepository = departmentReadRepository
                ?? throw new ArgumentNullException(nameof(departmentReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _departmentAdminReadRepository = departmentAdminReadRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

            _operatorReadRepository = operatorReadRepository
                ?? throw new ArgumentNullException(nameof(operatorReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetDepartmentDetailsResponse>> Handle(
            GetDepartmentDetailsQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetDepartmentDetailsResponse>.Fail(new Error(
                    Code: "Departments.Details.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<GetDepartmentDetailsResponse>.Fail(new Error(
                    Code: "Departments.Details.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.GetDepartmentDetails_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var department = await _departmentReadRepository.FirstOrDefaultAsync(
                new GetDepartmentBasicDetailsSpec(request.DepartmentId),
                cancellationToken);

            if (department is null)
            {
                return Result<GetDepartmentDetailsResponse>.Fail(new Error(
                    Code: "Departments.Details.DepartmentNotFound",
                    Message: ErrorMessage.GetDepartmentDetails_Department_NotFound,
                    Type: ErrorType.NotFound));
            }

            var departmentAdmins = await _departmentAdminReadRepository.ListAsync(
                new GetDepartmentAdminsForDepartmentDetailsSpec(request.DepartmentId),
                cancellationToken);

            var operators = await _operatorReadRepository.ListAsync(
                new GetOperatorsForDepartmentDetailsSpec(request.DepartmentId),
                cancellationToken);

            var response = new GetDepartmentDetailsResponse
            {
                Id = department.Id,
                NameEn = department.NameEn,
                NameAr = department.NameAr,
                IsActive = department.IsActive,
                CreatedOnUtc = department.CreatedOnUtc,

                Summary = new DepartmentDetailsSummaryResponse
                {
                    DepartmentAdminsCount = departmentAdmins.Count,
                    OperatorsCount = operators.Count
                },

                DepartmentAdmins = departmentAdmins,
                Operators = operators
            };

            return Result<GetDepartmentDetailsResponse>.Ok(response);
        }
    }
}