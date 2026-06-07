using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    internal sealed class GetTemplatesPaginationQueryHandler
        : IQueryHandler<GetTemplatesPaginationQuery, Pagination<TemplatePaginationItemResponse>>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;

        public GetTemplatesPaginationQueryHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<TemplatePaginationItemResponse>>> Handle(
            GetTemplatesPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<TemplatePaginationItemResponse>>.Fail(new Error(
                    Code: "Templates.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            request.SearchText ??= string.Empty;

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<Pagination<TemplatePaginationItemResponse>>.Fail(
                    currentBranchScope.Errors);
            }

            var branchId = currentBranchScope.Value.BranchId;

            var spec = new GetTemplatesPaginationSpec(
                branchId,
                request,
                request.IsActive);

            var (items, totalCount) = await _templateReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var creatorApplicationUserIds = items
                .Select(x => x.CreatedByApplicationUserId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<TemplatePaginationCreatorDto> creators;

            if (creatorApplicationUserIds.Length == 0)
            {
                creators = Array.Empty<TemplatePaginationCreatorDto>();
            }
            else
            {
                creators = await _applicationUserReadRepository.ListAsync(
                    new GetTemplateCreatorsForTemplatesPaginationSpec(creatorApplicationUserIds),
                    cancellationToken);
            }

            var creatorsByApplicationUserId = creators.ToDictionary(
                x => x.ApplicationUserId,
                x => x);

            var responseItems = items
                .Select(x =>
                {
                    creatorsByApplicationUserId.TryGetValue(
                        x.CreatedByApplicationUserId,
                        out var creator);

                    return new TemplatePaginationItemResponse
                    {
                        TemplateId = x.TemplateId,
                        BranchId = x.BranchId,
                        NameEn = x.NameEn,
                        NameAr = x.NameAr,
                        Description = x.Description,
                        Status = x.Status.ToString(),
                        IsActive = x.IsActive,
                        QuestionsCount = x.QuestionsCount,
                        CustomInputsCount = x.CustomInputsCount,
                        CreatedBy = creator is null
                            ? null
                            : new TemplatePaginationCreatedByResponse
                            {
                                NameEn = creator.NameEn,
                                NameAr = creator.NameAr
                            },
                        CreatedOnUtc = x.CreatedOnUtc,
                        ActiveFrom = x.ActiveFrom,
                        ExpireTo = x.ExpireTo
                    };
                })
                .ToArray();

            var response = new Pagination<TemplatePaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: responseItems);

            return Result<Pagination<TemplatePaginationItemResponse>>.Ok(response);
        }

    }
}
