using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Questions.Shared;

internal static class DropdownOptionValidation
{
    internal static IRuleBuilderOptionsConditions<T, IReadOnlyCollection<TOption>> ValidateDropdownOptions<T, TOption>(
        this IRuleBuilder<T, IReadOnlyCollection<TOption>> rule,
        Func<TOption, (string TextEn, string? TextAr, int Order, int Value)> fields)
        where TOption : class
    {
        return rule.Custom((options, context) =>
        {
            var path = context.PropertyPath;
            if (options is null || options.Count == 0)
            {
                context.AddFailure(path, ErrorMessage.Question_Dropdown_Options_Required);
                return;
            }

            var orders = new HashSet<int>();
            var englishLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var arabicLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var index = 0;

            foreach (var option in options)
            {
                var optionPath = $"{path}[{index++}]";
                if (option is null)
                {
                    context.AddFailure(optionPath, ErrorMessage.CreateQuestion_Option_TextEn_Required);
                    continue;
                }

                var (textEn, textAr, order, value) = fields(option);

                if (string.IsNullOrWhiteSpace(textEn))
                    context.AddFailure($"{optionPath}.TextEn", ErrorMessage.CreateQuestion_Option_TextEn_Required);
                else if (textEn.Length > 300)
                    context.AddFailure($"{optionPath}.TextEn", ErrorMessage.CreateQuestion_Option_TextEn_MaxLength);
                else if (!englishLabels.Add(textEn.Trim()))
                    context.AddFailure($"{optionPath}.TextEn", ErrorMessage.Question_Dropdown_Option_Labels_Duplicated);

                if (!string.IsNullOrWhiteSpace(textAr))
                {
                    if (textAr.Length > 300)
                        context.AddFailure($"{optionPath}.TextAr", ErrorMessage.CreateQuestion_Option_TextAr_MaxLength);
                    else if (!arabicLabels.Add(textAr.Trim()))
                        context.AddFailure($"{optionPath}.TextAr", ErrorMessage.Question_Dropdown_Option_Labels_Duplicated);
                }

                if (order <= 0)
                    context.AddFailure($"{optionPath}.Order", ErrorMessage.CreateQuestion_Option_Order_Invalid);
                else if (!orders.Add(order))
                    context.AddFailure($"{optionPath}.Order", ErrorMessage.CreateQuestion_Options_Order_Duplicated);

                if (value != 0)
                    context.AddFailure($"{optionPath}.Value", ErrorMessage.Question_Dropdown_Option_Value_Invalid);
            }
        });
    }
}
