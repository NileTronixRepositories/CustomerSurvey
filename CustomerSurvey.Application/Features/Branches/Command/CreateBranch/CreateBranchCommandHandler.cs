using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranch
{
    internal sealed class CreateBranchCommandHandler
        : ICommandHandler<CreateBranchCommand, CreateBranchResponse>
    {
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteRepository<Branch> _branchWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBranchCommandHandler(
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteRepository<Branch> branchWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _branchReadRepository = branchReadRepository
                ?? throw new ArgumentNullException(nameof(branchReadRepository));

            _branchWriteRepository = branchWriteRepository
                ?? throw new ArgumentNullException(nameof(branchWriteRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<CreateBranchResponse>> Handle(
            CreateBranchCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateBranchResponse>.Fail(new Error(
                    Code: "Branches.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<CreateBranchResponse>.Fail(new Error(
                    Code: "Branches.Create.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateBranch_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedCode = request.Code.Trim();

            var branchCodeExists = await _branchReadRepository.AnyAsync(
                x => x.Code == normalizedCode,
                cancellationToken);

            if (branchCodeExists)
            {
                return Result<CreateBranchResponse>.Fail(new Error(
                    Code: "Branches.Create.CodeAlreadyExists",
                    Message: ErrorMessage.CreateBranch_Code_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var branch = Branch.Create(
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                code: request.Code,
                address: request.Address,
                createdByApplicationUserId: currentApplicationUserId);

            await _branchWriteRepository.AddAsync(branch, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateBranchResponse
            {
                BranchId = branch.Id,
                NameEn = branch.NameEn,
                Code = branch.Code
            };

            return Result<CreateBranchResponse>.Ok(response);
        }
    }
}