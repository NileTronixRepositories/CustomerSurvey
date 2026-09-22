using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate
{
    internal sealed class CreateAnonymousTemplateCommandHandler
        : ICommandHandler<CreateAnonymousTemplateCommand, CreateAnonymousTemplateResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteRepository<AnonymousTemplate> _anonymousTemplateWriteRepository;
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IPublicSurveyUrlBuilder _publicSurveyUrlBuilder;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateAnonymousTemplateCommandHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteRepository<AnonymousTemplate> anonymousTemplateWriteRepository,
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IPublicSurveyUrlBuilder publicSurveyUrlBuilder,
            IQrCodeGenerator qrCodeGenerator,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousTemplateWriteRepository = anonymousTemplateWriteRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateWriteRepository));

            _branchReadRepository = branchReadRepository
                ?? throw new ArgumentNullException(nameof(branchReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _publicSurveyUrlBuilder = publicSurveyUrlBuilder
                ?? throw new ArgumentNullException(nameof(publicSurveyUrlBuilder));

            _qrCodeGenerator = qrCodeGenerator
                ?? throw new ArgumentNullException(nameof(qrCodeGenerator));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<CreateAnonymousTemplateResponse>> Handle(
            CreateAnonymousTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForCreateAnonymousTemplateSpec(currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is not null)
            {
                return await CreateGlobalAnonymousTemplateAsync(
                    request,
                    currentApplicationUserId,
                    cancellationToken);
            }

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<CreateAnonymousTemplateResponse>.Fail(currentBranchScope.Errors);
            }

            return await CreateBranchAnonymousTemplateAsync(
                request,
                currentBranchScope.Value.BranchId,
                currentApplicationUserId,
                cancellationToken);
        }

        private async Task<Result<CreateAnonymousTemplateResponse>> CreateGlobalAnonymousTemplateAsync(
            CreateAnonymousTemplateCommand request,
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            if (request.Scope.HasValue &&
                request.Scope.Value != AnonymousTemplateScope.Global)
            {
                return Result<CreateAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Create.SuperAdminCanCreateGlobalOnly",
                    Message: ErrorMessage.CreateAnonymousTemplate_SuperAdmin_GlobalOnly,
                    Type: ErrorType.Validation));
            }

            var normalizedNameEn = request.NameEn.Trim();

            var nameExists = await _anonymousTemplateReadRepository.AnyAsync(
                x => x.Scope == AnonymousTemplateScope.Global &&
                     x.NameEn == normalizedNameEn,
                cancellationToken);

            if (nameExists)
            {
                return Result<CreateAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Create.NameAlreadyExists",
                    Message: ErrorMessage.CreateAnonymousTemplate_Name_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var anonymousTemplate = AnonymousTemplate.CreateGlobalTemplate(
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                description: request.Description,
                activeFrom: request.ActiveFrom,
                expireTo: request.ExpireTo,
                createdByApplicationUserId: currentApplicationUserId);

            AddCustomInputs(
                anonymousTemplate,
                request.CustomInputs,
                currentApplicationUserId);

            await _anonymousTemplateWriteRepository.AddAsync(
                anonymousTemplate,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreateAnonymousTemplateResponse>.Ok(
                MapToResponse(anonymousTemplate, branch: null));
        }

        private async Task<Result<CreateAnonymousTemplateResponse>> CreateBranchAnonymousTemplateAsync(
            CreateAnonymousTemplateCommand request,
            Guid branchId,
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            if (request.Scope.HasValue &&
                request.Scope.Value != AnonymousTemplateScope.Branch)
            {
                return Result<CreateAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Create.BranchActorCanCreateBranchOnly",
                    Message: ErrorMessage.CreateAnonymousTemplate_BranchActor_BranchOnly,
                    Type: ErrorType.Validation));
            }

            var normalizedNameEn = request.NameEn.Trim();

            var nameExists = await _anonymousTemplateReadRepository.AnyAsync(
                x => x.Scope == AnonymousTemplateScope.Branch &&
                     x.BranchId == branchId &&
                     x.NameEn == normalizedNameEn,
                cancellationToken);

            if (nameExists)
            {
                return Result<CreateAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Create.NameAlreadyExists",
                    Message: ErrorMessage.CreateAnonymousTemplate_Name_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var anonymousTemplate = AnonymousTemplate.CreateBranchTemplate(
                branchId: branchId,
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                description: request.Description,
                activeFrom: request.ActiveFrom,
                expireTo: request.ExpireTo,
                createdByApplicationUserId: currentApplicationUserId);

            AddCustomInputs(
                anonymousTemplate,
                request.CustomInputs,
                currentApplicationUserId);

            SetPublicAccess(anonymousTemplate);

            await _anonymousTemplateWriteRepository.AddAsync(
                anonymousTemplate,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var branch = await _branchReadRepository.GetByPropertyAsync(
                x => x.Id == branchId,
                cancellationToken);

            return Result<CreateAnonymousTemplateResponse>.Ok(
                MapToResponse(anonymousTemplate, branch));
        }

        private void SetPublicAccess(AnonymousTemplate anonymousTemplate)
        {
            var publicUrl = _publicSurveyUrlBuilder.BuildAnonymousTemplateUrl(
                anonymousTemplate.Id);

            var qrCode = _qrCodeGenerator.GenerateBase64Png(publicUrl);

            anonymousTemplate.SetPublicAccess(
                publicUrl: publicUrl,
                qrCode: qrCode);
        }

        private static void AddCustomInputs(
            AnonymousTemplate anonymousTemplate,
            IReadOnlyCollection<CreateAnonymousTemplateCustomInputCommandItem> customInputs,
            Guid currentApplicationUserId)
        {
            foreach (var input in customInputs.OrderBy(x => x.Order))
            {
                var customInput = AnonymousTemplateCustomInput.Create(
                    anonymousTemplateId: anonymousTemplate.Id,
                    name: input.Name,
                    labelEn: input.LabelEn,
                    labelAr: input.LabelAr,
                    type: input.Type,
                    isRequired: input.IsRequired,
                    minLength: input.MinLength,
                    maxLength: input.MaxLength,
                    minValue: input.MinValue,
                    maxValue: input.MaxValue,
                    startWith: input.StartWith,
                    order: input.Order,
                    createdByApplicationUserId: currentApplicationUserId);

                anonymousTemplate.AddCustomInput(customInput);
            }
        }

        private static CreateAnonymousTemplateResponse MapToResponse(
            AnonymousTemplate anonymousTemplate,
            Branch? branch)
        {
            return new CreateAnonymousTemplateResponse
            {
                AnonymousTemplateId = anonymousTemplate.Id,
                BranchId = anonymousTemplate.BranchId,
                BranchNameEn = branch?.NameEn,
                BranchNameAr = branch?.NameAr,
                Scope = anonymousTemplate.Scope,
                ScopeName = anonymousTemplate.Scope.ToString(),
                IsGlobal = anonymousTemplate.Scope == AnonymousTemplateScope.Global,
                NameEn = anonymousTemplate.NameEn,
                NameAr = anonymousTemplate.NameAr,
                Description = anonymousTemplate.Description,
                ActiveFrom = anonymousTemplate.ActiveFrom,
                ExpireTo = anonymousTemplate.ExpireTo,
                IsActive = anonymousTemplate.IsActive,
                IsArchived = anonymousTemplate.IsArchived,
                LogoPath = anonymousTemplate.LogoPath,
                PublicUrl = anonymousTemplate.PublicUrl,
                QrCode = anonymousTemplate.QrCode,
                CreatedByApplicationUserId = anonymousTemplate.CreatedByApplicationUserId,
                CustomInputs = anonymousTemplate.CustomInputs
                    .OrderBy(x => x.Order)
                    .Select(x => new CreateAnonymousTemplateCustomInputResponse
                    {
                        CustomInputId = x.Id,
                        Name = x.Name,
                        LabelEn = x.LabelEn,
                        LabelAr = x.LabelAr,
                        Type = x.Type,
                        TypeName = x.Type.ToString(),
                        IsRequired = x.IsRequired,
                        MinLength = x.MinLength,
                        MaxLength = x.MaxLength,
                        MinValue = x.MinValue,
                        MaxValue = x.MaxValue,
                        StartWith = x.StartWith,
                        Order = x.Order,
                        IsActive = x.IsActive
                    })
                    .ToArray()
            };
        }
    }
}
