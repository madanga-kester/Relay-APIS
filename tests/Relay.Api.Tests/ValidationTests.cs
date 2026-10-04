using Relay.Application.Contracts;
using Relay.Application.Validation;
using Relay.Domain.Enums;

namespace Relay.Api.Tests;

public sealed class ValidationTests
{
    [Fact]
    public void RegisterRejectsWeakPasswords()
    {
        var errors = RequestValidators.Validate(new RegisterRequest("owner@example.com", "Owner", "weak", UserRole.Advertiser));
        Assert.Contains(errors, error => error.Contains("Password", StringComparison.Ordinal));
    }

    [Fact]
    public void CampaignRejectsBudgetBelowMaximumCommunityCost()
    {
        var errors = RequestValidators.Validate(new CreateCampaignRequest("Campaign", "Description", "Ad", "https://example.com", 2m, 10m, 6, "Category", "Kenya"));
        Assert.Contains(errors, error => error.Contains("budget", StringComparison.OrdinalIgnoreCase));
    }
}
