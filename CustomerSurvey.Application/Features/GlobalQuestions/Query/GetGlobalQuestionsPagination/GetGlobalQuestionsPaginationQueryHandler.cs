using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Application.Features.Questions.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionsPagination
{
    internal sealed class GetGlobalQuestionsPaginationQueryHandler
       : IQueryHandler<GetGlobalQuestionsPaginationQuery, Pagination<GlobalQuestionPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetGlobalQuestionsPaginationQueryHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<GlobalQuestionPaginationItemResponse>>> Handle(
            GetGlobalQuestionsPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<GlobalQuestionPaginationItemResponse>>.Fail(new Error(
                    Code: "GlobalQuestions.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            request.SearchText ??= string.Empty;

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForGetGlobalQuestionsPaginationSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<Pagination<GlobalQuestionPaginationItemResponse>>.Fail(new Error(
                    Code: "GlobalQuestions.Pagination.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.GetGlobalQuestionsPagination_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var spec = new GetGlobalQuestionsPaginationSpec(request);

            var (items, totalCount) = await _questionReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var questionIds = items
                .Where(x => x.Type == Domain.Enums.QuestionType.SingleChoice)
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<QuestionOptionResponse> options;

            if (questionIds.Length == 0)
            {
                options = Array.Empty<QuestionOptionResponse>();
            }
            else
            {
                options = await _questionOptionReadRepository.ListAsync(
                    new GetQuestionOptionsByQuestionIdsSpec(questionIds),
                    cancellationToken);
            }

            var optionsByQuestionId = options
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<QuestionOptionResponse>)x
                        .OrderBy(option => option.Order)
                        .ToArray());

            var itemsWithOptions = items
                .Select(item => item with
                {
                    Options = item.Type == Domain.Enums.QuestionType.SingleChoice &&
                              optionsByQuestionId.TryGetValue(
                                  item.QuestionId,
                                  out var itemOptions)
                        ? itemOptions
                        : Array.Empty<QuestionOptionResponse>()
                })
                .ToArray();

            var response = new Pagination<GlobalQuestionPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: itemsWithOptions);

            return Result<Pagination<GlobalQuestionPaginationItemResponse>>.Ok(response);
        }
    }
}