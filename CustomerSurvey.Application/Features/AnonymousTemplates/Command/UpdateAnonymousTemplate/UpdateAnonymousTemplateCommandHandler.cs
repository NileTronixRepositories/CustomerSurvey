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

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate
{
    internal sealed class UpdateAnonymousTemplateCommandHandler
        : ICommandHandler<UpdateAnonymousTemplateCommand, UpdateAnonymousTemplateResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteRepository<AnonymousTemplate> _anonymousTemplateWriteRepository;
        private readonly IWriteReadRepository<AnonymousTemplateCustomInput> _customInputReadRepository;
        private readonly IWriteRepository<AnonymousTemplateCustomInput> _customInputWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublicSurveyUrlBuilder _publicSurveyUrlBuilder;
        private readonly IQrCodeGenerator _qrCodeGenerator;

        public UpdateAnonymousTemplateCommandHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteRepository<AnonymousTemplate> anonymousTemplateWriteRepository,
            IWriteReadRepository<AnonymousTemplateCustomInput> customInputReadRepository,
            IWriteRepository<AnonymousTemplateCustomInput> customInputWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork,
            IPublicSurveyUrlBuilder publicSurveyUrlBuilder,
            IQrCodeGenerator qrCodeGenerator)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousTemplateWriteRepository = anonymousTemplateWriteRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateWriteRepository));

            _customInputReadRepository = customInputReadRepository
                ?? throw new ArgumentNullException(nameof(customInputReadRepository));

            _customInputWriteRepository = customInputWriteRepository
                ?? throw new ArgumentNullException(nameof(customInputWriteRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));

            _publicSurveyUrlBuilder = publicSurveyUrlBuilder
                ?? throw new ArgumentNullException(nameof(publicSurveyUrlBuilder));

            _qrCodeGenerator = qrCodeGenerator
                ?? throw new ArgumentNullException(nameof(qrCodeGenerator));
        }

        public async Task<Result<UpdateAnonymousTemplateResponse>> Handle(
            UpdateAnonymousTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

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
                    return Result<UpdateAnonymousTemplateResponse>.Fail(currentBranchScope.Errors);
                }

                currentBranchId = currentBranchScope.Value.BranchId;
            }

            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForUpdateSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<UpdateAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Update.TemplateNotFound",
                    Message: ErrorMessage.UpdateAnonymousTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!anonymousTemplate.IsActive)
            {
                return Result<UpdateAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Update.TemplateInactive",
                    Message: ErrorMessage.UpdateAnonymousTemplate_Template_Inactive,
                    Type: ErrorType.Validation));
            }

            var normalizedNameEn = request.NameEn.Trim();

            var nameExists = await _anonymousTemplateReadRepository.AnyAsync(
                x => x.Id != anonymousTemplate.Id &&
                     x.Scope == anonymousTemplate.Scope &&
                     x.BranchId == anonymousTemplate.BranchId &&
                     x.NameEn == normalizedNameEn,
                cancellationToken);

            if (nameExists)
            {
                return Result<UpdateAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Update.NameAlreadyExists",
                    Message: ErrorMessage.UpdateAnonymousTemplate_Name_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var existingCustomInputs = await _customInputReadRepository.ListAsync(
                new GetAnonymousTemplateCustomInputsForUpdateSpec(anonymousTemplate.Id),
                cancellationToken);

            var customInputsUpdateResult = ApplyCustomInputsUpdate(
                anonymousTemplateId: anonymousTemplate.Id,
                existingCustomInputs: existingCustomInputs,
                requestedCustomInputs: request.CustomInputs,
                currentApplicationUserId: currentApplicationUserId);

            if (customInputsUpdateResult.Error is not null)
            {
                return Result<UpdateAnonymousTemplateResponse>.Fail(
                    customInputsUpdateResult.Error);
            }

            anonymousTemplate.Update(
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                description: request.Description,
                activeFrom: request.ActiveFrom,
                expireTo: request.ExpireTo);

            RefreshPublicAccess(anonymousTemplate);

            _anonymousTemplateWriteRepository.Update(anonymousTemplate);

            if (customInputsUpdateResult.CustomInputsToUpdate.Count > 0)
            {
                _customInputWriteRepository.UpdateRange(
                    customInputsUpdateResult.CustomInputsToUpdate);
            }

            if (customInputsUpdateResult.NewCustomInputs.Count > 0)
            {
                await _customInputWriteRepository.AddRangeAsync(
                    customInputsUpdateResult.NewCustomInputs.ToList(),
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var activeCustomInputs = customInputsUpdateResult.CustomInputsToUpdate
                .Where(x => x.IsActive)
                .Concat(customInputsUpdateResult.NewCustomInputs)
                .OrderBy(x => x.Order)
                .ToArray();

            return Result<UpdateAnonymousTemplateResponse>.Ok(
                MapToResponse(anonymousTemplate, activeCustomInputs));
        }

        private void RefreshPublicAccess(AnonymousTemplate anonymousTemplate)
        {
            var publicUrl = _publicSurveyUrlBuilder.BuildAnonymousTemplateUrl(
                anonymousTemplate.Id);

            var qrCode = _qrCodeGenerator.GenerateBase64Png(publicUrl);

            anonymousTemplate.SetPublicAccess(
                publicUrl: publicUrl,
                qrCode: qrCode);
        }

        private static ApplyCustomInputsUpdateResult ApplyCustomInputsUpdate(
            Guid anonymousTemplateId,
            IReadOnlyCollection<AnonymousTemplateCustomInput> existingCustomInputs,
            IReadOnlyCollection<UpdateAnonymousTemplateCustomInputCommandItem> requestedCustomInputs,
            Guid currentApplicationUserId)
        {
            requestedCustomInputs ??= Array.Empty<UpdateAnonymousTemplateCustomInputCommandItem>();

            var existingCustomInputsList = existingCustomInputs.ToList();

            var finalActiveCustomInputIds = new HashSet<Guid>();
            var newCustomInputs = new List<AnonymousTemplateCustomInput>();

            foreach (var requestedCustomInput in requestedCustomInputs.OrderBy(x => x.Order))
            {
                var normalizedName = requestedCustomInput.Name.Trim();

                AnonymousTemplateCustomInput? customInput;

                if (requestedCustomInput.CustomInputId.HasValue)
                {
                    customInput = existingCustomInputsList.FirstOrDefault(
                        x => x.Id == requestedCustomInput.CustomInputId.Value);

                    if (customInput is null)
                    {
                        return ApplyCustomInputsUpdateResult.Fail(new Error(
                            Code: "AnonymousTemplates.Update.CustomInputNotFound",
                            Message: ErrorMessage.UpdateAnonymousTemplate_CustomInput_NotFound,
                            Type: ErrorType.Validation));
                    }
                }
                else
                {
                    customInput = existingCustomInputsList.FirstOrDefault(
                        x => string.Equals(
                            x.Name,
                            normalizedName,
                            StringComparison.OrdinalIgnoreCase));
                }

                if (customInput is not null)
                {
                    if (customInput.Type != requestedCustomInput.Type)
                    {
                        return ApplyCustomInputsUpdateResult.Fail(new Error(
                            Code: "AnonymousTemplates.Update.CustomInputTypeCannotBeChanged",
                            Message: ErrorMessage.UpdateAnonymousTemplate_CustomInput_Type_CannotBeChanged,
                            Type: ErrorType.Validation));
                    }

                    customInput.Update(
                        name: requestedCustomInput.Name,
                        labelEn: requestedCustomInput.LabelEn,
                        labelAr: requestedCustomInput.LabelAr,
                        type: requestedCustomInput.Type,
                        isRequired: requestedCustomInput.IsRequired,
                        minLength: requestedCustomInput.MinLength,
                        maxLength: requestedCustomInput.MaxLength,
                        minValue: requestedCustomInput.MinValue,
                        maxValue: requestedCustomInput.MaxValue,
                        startWith: requestedCustomInput.StartWith,
                        order: requestedCustomInput.Order);

                    customInput.Restore();

                    finalActiveCustomInputIds.Add(customInput.Id);
                    continue;
                }

                var newCustomInput = AnonymousTemplateCustomInput.Create(
                    anonymousTemplateId: anonymousTemplateId,
                    name: requestedCustomInput.Name,
                    labelEn: requestedCustomInput.LabelEn,
                    labelAr: requestedCustomInput.LabelAr,
                    type: requestedCustomInput.Type,
                    isRequired: requestedCustomInput.IsRequired,
                    minLength: requestedCustomInput.MinLength,
                    maxLength: requestedCustomInput.MaxLength,
                    minValue: requestedCustomInput.MinValue,
                    maxValue: requestedCustomInput.MaxValue,
                    startWith: requestedCustomInput.StartWith,
                    order: requestedCustomInput.Order,
                    createdByApplicationUserId: currentApplicationUserId);

                newCustomInputs.Add(newCustomInput);
                finalActiveCustomInputIds.Add(newCustomInput.Id);
            }

            foreach (var existingCustomInput in existingCustomInputsList)
            {
                if (existingCustomInput.IsActive &&
                    !finalActiveCustomInputIds.Contains(existingCustomInput.Id))
                {
                    existingCustomInput.Deactivate();
                }
            }

            return ApplyCustomInputsUpdateResult.Ok(
                customInputsToUpdate: existingCustomInputsList,
                newCustomInputs: newCustomInputs);
        }

        private static UpdateAnonymousTemplateResponse MapToResponse(
            AnonymousTemplate anonymousTemplate,
            IReadOnlyCollection<AnonymousTemplateCustomInput> activeCustomInputs)
        {
            return new UpdateAnonymousTemplateResponse
            {
                AnonymousTemplateId = anonymousTemplate.Id,
                BranchId = anonymousTemplate.BranchId,
                Scope = anonymousTemplate.Scope,
                ScopeName = anonymousTemplate.Scope.ToString(),
                IsGlobal = anonymousTemplate.Scope == AnonymousTemplateScope.Global,
                NameEn = anonymousTemplate.NameEn,
                NameAr = anonymousTemplate.NameAr,
                Description = anonymousTemplate.Description,
                ActiveFrom = anonymousTemplate.ActiveFrom,
                ExpireTo = anonymousTemplate.ExpireTo,
                Status = anonymousTemplate.Status,
                StatusName = anonymousTemplate.Status.ToString(),
                IsActive = anonymousTemplate.IsActive,
                PublicUrl = anonymousTemplate.PublicUrl,
                QrCode = anonymousTemplate.QrCode,
                CustomInputs = activeCustomInputs
                    .OrderBy(x => x.Order)
                    .Select(x => new UpdateAnonymousTemplateCustomInputResponse
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

        private sealed record ApplyCustomInputsUpdateResult(
            Error? Error,
            IReadOnlyCollection<AnonymousTemplateCustomInput> CustomInputsToUpdate,
            IReadOnlyCollection<AnonymousTemplateCustomInput> NewCustomInputs)
        {
            public static ApplyCustomInputsUpdateResult Ok(
                IReadOnlyCollection<AnonymousTemplateCustomInput> customInputsToUpdate,
                IReadOnlyCollection<AnonymousTemplateCustomInput> newCustomInputs)
            {
                return new ApplyCustomInputsUpdateResult(
                    Error: null,
                    CustomInputsToUpdate: customInputsToUpdate,
                    NewCustomInputs: newCustomInputs);
            }

            public static ApplyCustomInputsUpdateResult Fail(Error error)
            {
                return new ApplyCustomInputsUpdateResult(
                    Error: error,
                    CustomInputsToUpdate: Array.Empty<AnonymousTemplateCustomInput>(),
                    NewCustomInputs: Array.Empty<AnonymousTemplateCustomInput>());
            }
        }
    }
}
