using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    internal sealed class GetAnonymousTemplateResponsesPaginationQueryHandler
        : IQueryHandler<GetAnonymousTemplateResponsesPaginationQuery, Pagination<AnonymousTemplateResponsePaginationItemResponse>>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;

        public GetAnonymousTemplateResponsesPaginationQueryHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousSurveyResponseReadRepository = anonymousSurveyResponseReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>> Handle(
            GetAnonymousTemplateResponsesPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>.Fail(new Error(
                    Code: "AnonymousTemplates.ResponsesPagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            request.SearchText ??= string.Empty;

            var currentApplicationUserId = _currentUser.UserId.Value;

            var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            Guid? currentBranchId = null;

            if (!isSuperAdmin)
            {
                var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                    cancellationToken);

                if (currentBranchScope.IsFailure)
                {
                    return Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>.Fail(
                        currentBranchScope.Errors);
                }

                currentBranchId = currentBranchScope.Value.BranchId;
            }

            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForResponsesPaginationSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>.Fail(new Error(
                    Code: "AnonymousTemplates.ResponsesPagination.TemplateNotFound",
                    Message: ErrorMessage.GetAnonymousTemplateResponsesPagination_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var spec = new GetAnonymousTemplateResponsesPaginationSpec(request);

            var (items, totalCount) = await _anonymousSurveyResponseReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var response = new Pagination<AnonymousTemplateResponsePaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>.Ok(response);
        }

    }
}
