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

namespace CustomerSurvey.Application.Features.Branches.Command.UpdateBranch
{
    internal sealed class UpdateBranchCommandHandler
         : ICommandHandler<UpdateBranchCommand, UpdateBranchResponse>
    {
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteRepository<Branch> _branchWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateBranchCommandHandler(
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

        public async Task<Result<UpdateBranchResponse>> Handle(
            UpdateBranchCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateBranchResponse>.Fail(new Error(
                    Code: "Branches.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<UpdateBranchResponse>.Fail(new Error(
                    Code: "Branches.Update.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.UpdateBranch_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var branch = await _branchReadRepository.FirstOrDefaultAsync(
                new GetBranchForUpdateSpec(request.BranchId),
                cancellationToken);

            if (branch is null)
            {
                return Result<UpdateBranchResponse>.Fail(new Error(
                    Code: "Branches.Update.BranchNotFound",
                    Message: ErrorMessage.UpdateBranch_Branch_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedCode = request.Code.Trim();

            var codeAlreadyExists = await _branchReadRepository.AnyAsync(
                x => x.Id != request.BranchId &&
                     x.Code == normalizedCode,
                cancellationToken);

            if (codeAlreadyExists)
            {
                return Result<UpdateBranchResponse>.Fail(new Error(
                    Code: "Branches.Update.CodeAlreadyExists",
                    Message: ErrorMessage.UpdateBranch_Code_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            branch.Update(
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                code: request.Code,
                address: request.Address);

            _branchWriteRepository.Update(branch);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UpdateBranchResponse
            {
                BranchId = branch.Id,
                NameEn = branch.NameEn,
                NameAr = branch.NameAr,
                Code = branch.Code,
                Address = branch.Address,
                IsActive = branch.IsActive
            };

            return Result<UpdateBranchResponse>.Ok(response);
        }
    }
}