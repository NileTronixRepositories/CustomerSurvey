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

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsPagination
{
    internal sealed class GetGlobalQuestionGroupsPaginationQueryHandler
         : IQueryHandler<GetGlobalQuestionGroupsPaginationQuery, Pagination<GlobalQuestionGroupPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetGlobalQuestionGroupsPaginationQueryHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<GlobalQuestionGroupPaginationItemResponse>>> Handle(
            GetGlobalQuestionGroupsPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<GlobalQuestionGroupPaginationItemResponse>>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            request.SearchText ??= string.Empty;

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForGetGlobalQuestionGroupsPaginationSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<Pagination<GlobalQuestionGroupPaginationItemResponse>>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Pagination.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.GetGlobalQuestionGroupsPagination_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var spec = new GetGlobalQuestionGroupsPaginationSpec(request);

            var (items, totalCount) = await _questionGroupReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var response = new Pagination<GlobalQuestionGroupPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<GlobalQuestionGroupPaginationItemResponse>>.Ok(response);
        }
    }
}