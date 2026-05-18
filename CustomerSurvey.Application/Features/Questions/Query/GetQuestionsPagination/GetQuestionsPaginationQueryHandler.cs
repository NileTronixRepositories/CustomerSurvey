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

namespace CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination
{
    internal sealed class GetQuestionsPaginationQueryHandler
        : IQueryHandler<GetQuestionsPaginationQuery, Pagination<QuestionPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetQuestionsPaginationQueryHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            ICurrentUser currentUser)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

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

            var questionIds = items
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

            var creatorApplicationUserIds = items
                .Select(x => x.CreatedByApplicationUserId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<QuestionPaginationCreatorDto> creators;

            if (creatorApplicationUserIds.Length == 0)
            {
                creators = Array.Empty<QuestionPaginationCreatorDto>();
            }
            else
            {
                creators = await _applicationUserReadRepository.ListAsync(
                    new GetQuestionCreatorsForQuestionsPaginationSpec(creatorApplicationUserIds),
                    cancellationToken);
            }

            var creatorsByApplicationUserId = creators.ToDictionary(
                x => x.ApplicationUserId,
                x => x);

            var responseItems = items
                .Select(question =>
                {
                    optionsByQuestionId.TryGetValue(
                        question.QuestionId,
                        out var questionOptions);

                    creatorsByApplicationUserId.TryGetValue(
                        question.CreatedByApplicationUserId,
                        out var creator);

                    return new QuestionPaginationItemResponse
                    {
                        QuestionId = question.QuestionId,
                        BranchId = question.BranchId,
                        GroupId = question.GroupId,
                        GroupNameEn = question.GroupNameEn,
                        GroupNameAr = question.GroupNameAr,
                        TextEn = question.TextEn,
                        TextAr = question.TextAr,
                        Type = question.Type,
                        TypeName = question.TypeName,
                        IsActive = question.IsActive,
                        CreatedBy = creator is null
                            ? null
                            : new QuestionPaginationCreatedByResponse
                            {
                                NameEn = creator.NameEn,
                                NameAr = creator.NameAr
                            },
                        CreatedOnUtc = question.CreatedOnUtc,
                        Options = questionOptions ?? Array.Empty<QuestionOptionResponse>()
                    };
                })
                .ToArray();

            var response = new Pagination<QuestionPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: responseItems);

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