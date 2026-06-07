using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.BranchAreas.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.AssignBranchesToBranchArea
{
    internal sealed class AssignBranchesToBranchAreaCommandHandler
        : ICommandHandler<AssignBranchesToBranchAreaCommand, AssignBranchesToBranchAreaResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchArea> _branchAreaReadRepository;
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteReadRepository<BranchAreaBranch> _branchAreaBranchReadRepository;
        private readonly IWriteRepository<BranchAreaBranch> _branchAreaBranchWriteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AssignBranchesToBranchAreaCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchArea> branchAreaReadRepository,
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteReadRepository<BranchAreaBranch> branchAreaBranchReadRepository,
            IWriteRepository<BranchAreaBranch> branchAreaBranchWriteRepository,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _branchAreaReadRepository = branchAreaReadRepository ?? throw new ArgumentNullException(nameof(branchAreaReadRepository));
            _branchReadRepository = branchReadRepository ?? throw new ArgumentNullException(nameof(branchReadRepository));
            _branchAreaBranchReadRepository = branchAreaBranchReadRepository ?? throw new ArgumentNullException(nameof(branchAreaBranchReadRepository));
            _branchAreaBranchWriteRepository = branchAreaBranchWriteRepository ?? throw new ArgumentNullException(nameof(branchAreaBranchWriteRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<AssignBranchesToBranchAreaResponse>> Handle(
            AssignBranchesToBranchAreaCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<AssignBranchesToBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.AssignBranches.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var superAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == _currentUser.UserId.Value,
                cancellationToken);

            if (!superAdminExists)
            {
                return Result<AssignBranchesToBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.AssignBranches.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var branchArea = await _branchAreaReadRepository.FirstOrDefaultAsync(
                new GetBranchAreaForManagementSpec(request.BranchAreaId),
                cancellationToken);

            if (branchArea is null)
            {
                return Result<AssignBranchesToBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.AssignBranches.BranchAreaNotFound",
                    Message: ErrorMessage.AssignBranchAreaBranches_BranchArea_NotFound,
                    Type: ErrorType.NotFound));
            }

            var requestedBranchIds = request.BranchIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToArray();

            if (requestedBranchIds.Length == 0)
            {
                return Result<AssignBranchesToBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.AssignBranches.BranchesRequired",
                    Message: ErrorMessage.AssignBranchAreaBranches_Branches_Required,
                    Type: ErrorType.Validation));
            }

            if (requestedBranchIds.Length != request.BranchIds.Count)
            {
                return Result<AssignBranchesToBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.AssignBranches.BranchIdsDuplicatedOrInvalid",
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
                return Result<AssignBranchesToBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.AssignBranches.BranchesNotFound",
                    Message: ErrorMessage.AssignBranchAreaBranches_Branches_NotFound,
                    Type: ErrorType.NotFound));
            }

            var currentAssignments = await _branchAreaBranchReadRepository.ListAsync(
                new GetCurrentBranchAreaBranchesForAssignSpec(branchArea.BranchAreaId),
                cancellationToken);

            _branchAreaBranchWriteRepository.DeleteRange(currentAssignments);

            var newAssignments = requestedBranchIds
                .Select(branchId => BranchAreaBranch.Create(
                    branchAreaId: branchArea.BranchAreaId,
                    branchId: branchId,
                    createdByApplicationUserId: _currentUser.UserId.Value))
                .ToList();

            await _branchAreaBranchWriteRepository.AddRangeAsync(newAssignments, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AssignBranchesToBranchAreaResponse>.Ok(new AssignBranchesToBranchAreaResponse
            {
                BranchAreaId = branchArea.BranchAreaId,
                Branches = branches
            });
        }
    }
}
