using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination
{
    internal sealed class GetQuestionsPaginationQueryHandler
        : IQueryHandler<GetQuestionsPaginationQuery, Pagination<QuestionPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetQuestionsPaginationQueryHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<QuestionPaginationItemResponse>>> Handle(
            GetQuestionsPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<QuestionPaginationItemResponse>>.Fail(new Error(
                    Code: "Questions.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchId = await ResolveActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (!actorBranchId.HasValue)
            {
                return Result<Pagination<QuestionPaginationItemResponse>>.Fail(new Error(
                    Code: "Questions.Pagination.CurrentBranchActorNotFound",
                    Message: ErrorMessage.GetQuestionsPagination_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            request.SearchText ??= string.Empty;

            var spec = new GetQuestionsPaginationSpec(
                branchId: actorBranchId.Value,
                searchParameters: request);

            var (items, totalCount) = await _questionReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var response = new Pagination<QuestionPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<QuestionPaginationItemResponse>>.Ok(response);
        }

        private async Task<Guid?> ResolveActorBranchIdAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForQuestionPaginationSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin.BranchId;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForQuestionPaginationSpec(applicationUserId),
                cancellationToken);

            return branchUser?.BranchId;
        }
    }
}