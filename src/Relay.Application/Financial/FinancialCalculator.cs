namespace Relay.Application.Financial;

public sealed record FinancialBreakdown(
    decimal AdvertiserCpc,
    decimal CommunityOwnerCpc,
    decimal PlatformFeePerClick,
    int MaximumQualifiedClicks,
    int QualifiedClicks,
    decimal AdvertiserSpend,
    decimal CommunityOwnerEarnings,
    decimal PlatformRevenue,
    decimal RemainingBudget);

public interface IFinancialCalculator
{
    FinancialBreakdown Calculate(decimal cpc, decimal budget, int qualifiedClicks);
}

public sealed class FinancialCalculator : IFinancialCalculator
{
    public const decimal PlatformFeeRate = 0.25m;

    public FinancialBreakdown Calculate(decimal cpc, decimal budget, int qualifiedClicks)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cpc);
        ArgumentOutOfRangeException.ThrowIfNegative(budget);
        var maximum = cpc <= 0 ? 0 : (int)Math.Floor(budget / cpc);
        var capped = Math.Clamp(qualifiedClicks, 0, maximum);
        var platformFee = decimal.Round(cpc * PlatformFeeRate, 2, MidpointRounding.ToEven);
        var ownerCpc = Math.Max(cpc - platformFee, 0);
        var spend = decimal.Round(capped * cpc, 2, MidpointRounding.ToEven);
        return new FinancialBreakdown(cpc, ownerCpc, platformFee, maximum, capped, spend,
            decimal.Round(capped * ownerCpc, 2, MidpointRounding.ToEven),
            decimal.Round(capped * platformFee, 2, MidpointRounding.ToEven), Math.Max(budget - spend, 0));
    }
}
