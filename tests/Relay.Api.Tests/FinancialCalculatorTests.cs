using Relay.Application.Financial;

namespace Relay.Api.Tests;

public sealed class FinancialCalculatorTests
{
    private readonly FinancialCalculator calculator = new();

    [Fact]
    public void SplitsEachQualifiedClickIntoOwnerAndPlatformAmounts()
    {
        var result = calculator.Calculate(2m, 100m, 3);
        Assert.Equal(1.50m, result.CommunityOwnerCpc);
        Assert.Equal(0.50m, result.PlatformFeePerClick);
        Assert.Equal(6m, result.AdvertiserSpend);
        Assert.Equal(4.50m, result.CommunityOwnerEarnings);
        Assert.Equal(1.50m, result.PlatformRevenue);
        Assert.Equal(result.AdvertiserSpend, result.CommunityOwnerEarnings + result.PlatformRevenue);
    }

    [Fact]
    public void CapsQualifiedClicksAtFloorBudgetDividedByCpc()
    {
        var result = calculator.Calculate(2m, 5m, 99);
        Assert.Equal(2, result.MaximumQualifiedClicks);
        Assert.Equal(2, result.QualifiedClicks);
        Assert.Equal(4m, result.AdvertiserSpend);
        Assert.Equal(1m, result.RemainingBudget);
    }
}
