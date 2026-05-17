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

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate
{
    internal sealed class UpdateTemplateCommandHandler
         : ICommandHandler<UpdateTemplateCommand, UpdateTemplateResponse>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteRepository<Template> _templateWriteRepository;

        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;

        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTemplateCommandHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteRepository<Template> templateWriteRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _templateWriteRepository = templateWriteRepository
                ?? throw new ArgumentNullException(nameof(templateWriteRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<UpdateTemplateResponse>> Handle(
            UpdateTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateTemplateResponse>.Fail(new Error(
                    Code: "Templates.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            Guid? actorBranchId = null;

            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForUpdateTemplateSpec(currentApplicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                actorBranchId = branchAdmin.BranchId;
            }
            else
            {
                var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                    new GetCurrentBranchUserForUpdateTemplateSpec(currentApplicationUserId),
                    cancellationToken);

                if (branchUser is not null)
                {
                    actorBranchId = branchUser.BranchId;
                }
            }

            if (!actorBranchId.HasValue)
            {
                return Result<UpdateTemplateResponse>.Fail(new Error(
                    Code: "Templates.Update.CurrentBranchActorNotFound",
                    Message: ErrorMessage.UpdateTemplate_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            var branchId = actorBranchId.Value;

            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateForUpdateSpec(
                    templateId: request.TemplateId,
                    branchId: branchId),
                cancellationToken);

            if (template is null)
            {
                return Result<UpdateTemplateResponse>.Fail(new Error(
                    Code: "Templates.Update.TemplateNotFound",
                    Message: ErrorMessage.UpdateTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedNameEn = request.NameEn.Trim();

            var nameEnExists = await _templateReadRepository.AnyAsync(
                x =>
                    x.BranchId == branchId &&
                    x.Id != request.TemplateId &&
                    x.NameEn == normalizedNameEn,
                cancellationToken);

            if (nameEnExists)
            {
                return Result<UpdateTemplateResponse>.Fail(new Error(
                    Code: "Templates.Update.NameEnAlreadyExistsInsideBranch",
                    Message: ErrorMessage.UpdateTemplate_NameEn_AlreadyExists_InsideBranch,
                    Type: ErrorType.Validation));
            }

            template.Update(
     nameEn: normalizedNameEn,
     nameAr: request.NameAr,
     description: request.Description,
     activeFrom: request.ActiveFrom,
     expireTo: request.ExpireTo);

            _templateWriteRepository.Update(template);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UpdateTemplateResponse
            {
                TemplateId = template.Id,
                BranchId = template.BranchId,
                NameEn = template.NameEn,
                NameAr = template.NameAr,
                Description = template.Description,
                ActiveFrom = template.ActiveFrom,
                ExpireTo = template.ExpireTo,
                Status = template.Status.ToString(),
                IsActive = template.IsActive
            };

            return Result<UpdateTemplateResponse>.Ok(response);
        }
    }
}