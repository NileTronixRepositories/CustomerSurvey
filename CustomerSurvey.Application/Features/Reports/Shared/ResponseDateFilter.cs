using System.Linq.Expressions;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Shared;

internal static class ResponseDateFilter
{
    public static Expression<Func<SurveyResponse, bool>> ForSurvey(
        ResponseDateBasis basis, DateTime? fromUtc, DateTime? toExclusiveUtc)
    {
        if (basis == ResponseDateBasis.SubmittedOnUtc)
        {
            return response => (!fromUtc.HasValue || response.SubmittedOnUtc >= fromUtc) &&
                               (!toExclusiveUtc.HasValue || response.SubmittedOnUtc < toExclusiveUtc);
        }

        var from = ToDate(fromUtc);
        var toExclusive = ToDate(toExclusiveUtc);

        return response => response.CustomInputValues.Any(value =>
            value.TemplateCustomInput.SystemFieldKey == TemplateCustomInputSystemField.VisitTime &&
            value.DateValue.HasValue &&
            (!from.HasValue || value.DateValue >= from) &&
            (!toExclusive.HasValue || value.DateValue < toExclusive));
    }

    public static Expression<Func<AnonymousSurveyResponse, bool>> ForAnonymous(
        ResponseDateBasis basis, DateTime? fromUtc, DateTime? toExclusiveUtc)
    {
        if (basis == ResponseDateBasis.SubmittedOnUtc)
        {
            return response => (!fromUtc.HasValue || response.SubmittedOnUtc >= fromUtc) &&
                               (!toExclusiveUtc.HasValue || response.SubmittedOnUtc < toExclusiveUtc);
        }

        var from = ToDate(fromUtc);
        var toExclusive = ToDate(toExclusiveUtc);

        return response => response.CustomInputValues.Any(value =>
            value.AnonymousTemplateCustomInput.SystemFieldKey == TemplateCustomInputSystemField.VisitTime &&
            value.DateValue.HasValue &&
            (!from.HasValue || value.DateValue >= from) &&
            (!toExclusive.HasValue || value.DateValue < toExclusive));
    }

    private static DateOnly? ToDate(DateTime? value)
        => value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
}
