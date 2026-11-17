using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Questions.Shared;

internal sealed class QuestionDefinitionUpdater(
    IWriteRepository<Question> questions,
    IWriteReadRepository<QuestionOption> optionReader,
    IWriteRepository<QuestionOption> options,
    IWriteReadRepository<TemplateQuestionCondition> authorizedConditions,
    IWriteReadRepository<AnonymousTemplateQuestionCondition> anonymousConditions,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<QuestionOptionResponse[]>> UpdateAsync(
        Question question,
        Guid groupId,
        string textEn,
        string? textAr,
        QuestionType type,
        IReadOnlyCollection<QuestionOptionCommandItem> requestedOptions,
        Guid actorId,
        CancellationToken ct)
    {
        var isGlobal = question.Scope == QuestionScope.Global;
        var errorPrefix = isGlobal ? "GlobalQuestions.Update" : "Questions.Update";

        if (question.Type != type && await HasIncompatibleParentConditionsAsync(question.Id, type, ct))
        {
            return Invalid("TypeChangeIncompatibleWithConditions", ErrorMessage.Question_TypeChange_IncompatibleWithConditions);
        }

        var currentOptions = await optionReader.ListAsync(new GetQuestionOptionsForUpdateQuestionSpec(question.Id), ct);
        var currentById = currentOptions.ToDictionary(x => x.Id);
        if (!type.HasOptions())
        {
            requestedOptions = Array.Empty<QuestionOptionCommandItem>();
        }

        var existingIds = requestedOptions.Where(x => x.OptionId.HasValue).Select(x => x.OptionId!.Value).ToArray();
        var retainedIds = existingIds.ToHashSet();
        if (retainedIds.Count != existingIds.Length)
        {
            return Invalid("OptionIdDuplicated", isGlobal
                ? ErrorMessage.UpdateGlobalQuestion_OptionId_Duplicated
                : ErrorMessage.UpdateQuestion_OptionId_Duplicated);
        }

        if (retainedIds.Any(id => !currentById.ContainsKey(id)))
        {
            return Invalid("OptionNotBelongToQuestion", isGlobal
                ? ErrorMessage.UpdateGlobalQuestion_Option_NotBelongToQuestion
                : ErrorMessage.UpdateQuestion_Option_NotBelongToQuestion);
        }

        var omittedOptions = currentOptions.Where(x => !retainedIds.Contains(x.Id)).ToArray();
        var removedOptions = omittedOptions.Where(x => x.IsActive).ToArray();
        // Earlier updates could leave an active anonymous condition pointing to an inactive option.
        var returningToSingleChoice = type == QuestionType.SingleChoice && question.Type != type;
        var protectedOptionIds = (returningToSingleChoice ? omittedOptions : removedOptions).Select(x => x.Id).ToArray();
        if (protectedOptionIds.Length > 0 && await AreOptionsUsedByConditionsAsync(protectedOptionIds, ct))
        {
            return Invalid("OptionUsedInCondition", isGlobal
                ? ErrorMessage.UpdateGlobalQuestion_Option_UsedInCondition
                : ErrorMessage.UpdateQuestion_Option_UsedInCondition);
        }

        if (requestedOptions.Select(x => x.Order).Distinct().Count() != requestedOptions.Count)
        {
            return Invalid("OptionOrderDuplicated", ErrorMessage.UpdateQuestion_Options_Order_Duplicated);
        }

        if (type == QuestionType.Dropdown && requestedOptions.Any(x => x.Value != 0))
        {
            return Invalid("DropdownOptionValueInvalid", ErrorMessage.Question_Dropdown_Option_Value_Invalid);
        }

        var requestedByOrder = requestedOptions.ToDictionary(x => x.Order);
        var displacedOptions = currentOptions
            .Where(option => requestedByOrder.TryGetValue(option.Order, out var request) && request.OptionId != option.Id)
            .OrderBy(option => option.Order)
            .ToArray();

        // The unique order index includes inactive rows. Free occupied slots before reusing them,
        // and commit both saves together so a failed update cannot leave options relocated.
        await using var transaction = displacedOptions.Length > 0 ? await unitOfWork.BeginTransactionAsync(ct) : null;
        if (transaction is not null)
        {
            var reservedOrders = currentOptions.Select(x => x.Order).Concat(requestedByOrder.Keys).ToHashSet();
            var spareOrder = 1;
            foreach (var option in displacedOptions)
            {
                while (!reservedOrders.Add(spareOrder))
                {
                    spareOrder++;
                }

                option.ChangeOrder(spareOrder);
                options.Update(option);
            }

            await unitOfWork.SaveChangesAsync(ct);
        }

        foreach (var option in removedOptions)
        {
            option.Deactivate();
            options.Update(option);
        }

        var activeOptions = new List<QuestionOptionResponse>(requestedOptions.Count);
        foreach (var request in requestedOptions.OrderBy(x => x.Order))
        {
            QuestionOption option;
            if (request.OptionId is Guid optionId)
            {
                option = currentById[optionId];
                option.Update(request.TextEn, request.TextAr, request.Order, request.Value);
                option.Restore();
                options.Update(option);
            }
            else
            {
                option = QuestionOption.Create(question.Id, request.TextEn, request.TextAr, request.Order, request.Value, actorId);
                await options.AddAsync(option, ct);
            }

            activeOptions.Add(new QuestionOptionResponse
            {
                OptionId = option.Id,
                QuestionId = option.QuestionId,
                TextEn = option.TextEn,
                TextAr = option.TextAr,
                Order = option.Order,
                Value = option.Value,
                IsActive = option.IsActive
            });
        }

        question.Update(groupId, textEn, textAr, type);
        questions.Update(question);
        await unitOfWork.SaveChangesAsync(ct);
        if (transaction is not null)
        {
            await transaction.CommitAsync(ct);
        }

        return Result<QuestionOptionResponse[]>.Ok(activeOptions.ToArray());

        Result<QuestionOptionResponse[]> Invalid(string code, string message) =>
            Result<QuestionOptionResponse[]>.Fail(new Error($"{errorPrefix}.{code}", message, ErrorType.Validation));
    }

    private async Task<bool> HasIncompatibleParentConditionsAsync(Guid questionId, QuestionType type, CancellationToken ct)
    {
        QuestionConditionTriggerType? compatibleTrigger = type switch
        {
            QuestionType.SingleChoice => QuestionConditionTriggerType.SingleChoiceOption,
            QuestionType.StarRating => QuestionConditionTriggerType.StarRatingValue,
            QuestionType.Smiles => QuestionConditionTriggerType.SmileValue,
            _ => null
        };

        return await authorizedConditions.AnyAsync(x => x.IsActive &&
                   x.ParentTemplateQuestion.QuestionId == questionId && x.TriggerType != compatibleTrigger, ct)
            || await anonymousConditions.AnyAsync(x => x.IsActive &&
                   x.ParentAnonymousTemplateQuestion.QuestionId == questionId && x.TriggerType != compatibleTrigger, ct);
    }

    private async Task<bool> AreOptionsUsedByConditionsAsync(Guid[] optionIds, CancellationToken ct) =>
        await authorizedConditions.AnyAsync(x => x.IsActive && x.SelectedQuestionOptionId.HasValue &&
            optionIds.Contains(x.SelectedQuestionOptionId.Value), ct)
        || await anonymousConditions.AnyAsync(x => x.IsActive && x.SelectedQuestionOptionId.HasValue &&
            optionIds.Contains(x.SelectedQuestionOptionId.Value), ct);
}
