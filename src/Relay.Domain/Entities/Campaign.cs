using Relay.Domain.Enums;

namespace Relay.Domain.Entities;

public sealed class Campaign : AuditableEntity
{
    private Campaign() { }

    public Campaign(Guid advertiserId, string name, string advertiserName, string description, string advertisement, string destinationUrl,
        IReadOnlyCollection<string> platforms, int minimumAudience, int maximumAudience, string category, string location,
        int durationDays, int maximumCommunities, decimal cpc, decimal budget, DateOnly startDate, DateOnly endDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cpc);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(budget);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumCommunities);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumAudience);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumAudience);
        if (maximumAudience < minimumAudience) throw new ArgumentException("Maximum audience must be at least the minimum audience.");
        if (endDate < startDate) throw new ArgumentException("Campaign end date must not precede its start date.");
        if (platforms.Count == 0) throw new ArgumentException("At least one target platform is required.");
        AdvertiserId = advertiserId;
        Name = name.Trim();
        AdvertiserName = advertiserName.Trim();
        Description = description.Trim();
        Advertisement = advertisement.Trim();
        DestinationUrl = destinationUrl.Trim();
        Platforms = platforms.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        MinimumAudience = minimumAudience;
        MaximumAudience = maximumAudience;
        Category = category.Trim();
        Location = location.Trim();
        DurationDays = durationDays;
        MaximumCommunities = maximumCommunities;
        Cpc = cpc;
        Budget = budget;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Campaign(Guid advertiserId, string name, string description, string advertisement, string destinationUrl, decimal cpc, decimal budget, int maximumCommunities, string category, string location)
        : this(advertiserId, name, name, description, advertisement, destinationUrl, ["WhatsApp"], 1, int.MaxValue, category, location, 7, maximumCommunities, cpc, budget, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))) { }

    public Guid AdvertiserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string AdvertiserName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Advertisement { get; private set; } = string.Empty;
    public string DestinationUrl { get; private set; } = string.Empty;
    public string[] Platforms { get; private set; } = [];
    public int MinimumAudience { get; private set; }
    public int MaximumAudience { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public int DurationDays { get; private set; }
    public int MaximumCommunities { get; private set; }
    public decimal Cpc { get; private set; }
    public decimal Budget { get; private set; }
    public CampaignStatus Status { get; private set; } = CampaignStatus.Draft;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }

    public void Publish() { if (Status != CampaignStatus.Draft) throw new InvalidOperationException("Only Draft campaigns can be published."); Status = CampaignStatus.Published; }
    public void ActivateFromPlacement() { if (Status == CampaignStatus.Published) Status = CampaignStatus.Active; }
    public void Pause() { if (Status != CampaignStatus.Active) throw new InvalidOperationException("Only Active campaigns can be paused."); Status = CampaignStatus.Paused; }
    public void Resume() { if (Status != CampaignStatus.Paused) throw new InvalidOperationException("Only Paused campaigns can be resumed."); Status = CampaignStatus.Active; }
    public void Complete() { if (Status is not (CampaignStatus.Active or CampaignStatus.Paused)) throw new InvalidOperationException("Only Active or Paused campaigns can be completed."); Status = CampaignStatus.Completed; }
    public void MarkBudgetExhausted() { if (Status == CampaignStatus.Active) Status = CampaignStatus.BudgetExhausted; }
    public void UpdateDraft(string name, string advertiserName, string description, string advertisement, string destinationUrl, IReadOnlyCollection<string> platforms, int minimumAudience, int maximumAudience, string category, string location, int durationDays, int maximumCommunities, decimal cpc, decimal budget, DateOnly startDate, DateOnly endDate)
    {
        if (Status != CampaignStatus.Draft) throw new InvalidOperationException("Only Draft campaigns can be edited safely.");
        if (maximumAudience < minimumAudience || platforms.Count == 0 || cpc <= 0 || budget <= 0 || maximumCommunities <= 0 || endDate < startDate) throw new ArgumentException("Campaign values are invalid.");
        Name = name.Trim(); AdvertiserName = advertiserName.Trim(); Description = description.Trim(); Advertisement = advertisement.Trim(); DestinationUrl = destinationUrl.Trim(); Platforms = platforms.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(); MinimumAudience = minimumAudience; MaximumAudience = maximumAudience; Category = category.Trim(); Location = location.Trim(); DurationDays = durationDays; MaximumCommunities = maximumCommunities; Cpc = cpc; Budget = budget; StartDate = startDate; EndDate = endDate;
    }
}
