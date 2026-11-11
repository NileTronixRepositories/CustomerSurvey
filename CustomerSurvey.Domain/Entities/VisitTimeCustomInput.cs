namespace CustomerSurvey.Domain.Entities;

public static class VisitTimeCustomInput
{
    internal const string LabelEn = "Visit Time";
    internal const string LabelAr = "تاريخ الزيارة";

    public static int GetAvailableOrder(IEnumerable<int> otherOrders)
    {
        var orders = otherOrders.ToHashSet();
        var lastOrder = orders.DefaultIfEmpty(0).Max();
        if (lastOrder < int.MaxValue)
        {
            return lastOrder + 1;
        }

        var order = 1;
        while (orders.Contains(order))
        {
            order++;
        }

        return order;
    }
}
