using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.BranchAreas.Shared.Specs;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.BranchAreas.Query.GetBranchAreaDetails
{
    internal sealed class GetBranchAreaDetailsQueryHandler
        : IQueryHandler<GetBranchAreaDetailsQuery, BranchAreaDetailsResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchArea> _branchAreaReadRepository;
        private readonly IWriteReadRepository<BranchAreaBranch> _branchAreaBranchReadRepository;

        public GetBranchAreaDetailsQueryHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchArea> branchAreaReadRepository,
            IWriteReadRepository<BranchAreaBranch> branchAreaBranchReadRepository)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _branchAreaReadRepository = branchAreaReadRepository ?? throw new ArgumentNullException(nameof(branchAreaReadRepository));
            _branchAreaBranchReadRepository = branchAreaBranchReadRepository ?? throw new ArgumentNullException(nameof(branchAreaBranchReadRepository));
        }

        public async Task<Result<BranchAreaDetailsResponse>> Handle(
            GetBranchAreaDetailsQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<BranchAreaDetailsResponse>.Fail(new Error(
                    Code: "BranchAreas.Details.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var superAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == _currentUser.UserId.Value,
                cancellationToken);

            if (!superAdminExists)
            {
                return Result<BranchAreaDetailsResponse>.Fail(new Error(
                    Code: "BranchAreas.Details.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var details = await _branchAreaReadRepository.FirstOrDefaultAsync(
                new GetBranchAreaDetailsSpec(request.BranchAreaId),
                cancellationToken);

            if (details is null)
            {
                return Result<BranchAreaDetailsResponse>.Fail(new Error(
                    Code: "BranchAreas.Details.BranchAreaNotFound",
                    Message: ErrorMessage.AssignBranchAreaBranches_BranchArea_NotFound,
                    Type: ErrorType.NotFound));
            }

            var branches = await _branchAreaBranchReadRepository.ListAsync(
                new GetBranchAreaBranchesSpec(details.BranchAreaId),
                cancellationToken);

            return Result<BranchAreaDetailsResponse>.Ok(new BranchAreaDetailsResponse
            {
                BranchAreaId = details.BranchAreaId,
                ApplicationUserId = details.ApplicationUserId,
                NameEn = details.NameEn,
                NameAr = details.NameAr,
                UserName = details.UserName,
                Email = details.Email,
                PhoneNumber = details.PhoneNumber,
                IsActive = details.IsActive,
                CreatedOnUtc = details.CreatedOnUtc,
                Branches = branches
            });
        }
    }
}
