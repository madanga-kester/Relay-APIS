namespace Relay.Domain.Enums;

public enum UserRole
{
    Advertiser = 1,
    CommunityOwner = 2,
    Admin = 3
}

public enum AccountStatus
{
    Active = 1,
    Suspended = 2,
    PendingVerification = 3
}

public enum CampaignStatus
{
    Draft = 1,
    Published = 2,
    Active = 3,
    Paused = 4,
    Completed = 5,
    BudgetExhausted = 6
}

public enum ApplicationStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}

public enum PlacementStatus
{
    ReadyToPost = 1,
    Active = 2,
    Completed = 3
}

public enum CommunityPlatform
{
    WhatsApp = 1,
    Telegram = 2,
    Discord = 3,
    Facebook = 4,
    Instagram = 5,
    Other = 6,
    Newsletter = 7
}

public enum VerificationStatus
{
    Pending = 1,
    Verified = 2,
    Suspended = 3
}

public enum ClickQualification
{
    Qualified = 1,
    Rejected = 2
}

public enum LedgerEntryType
{
    QualifiedClick = 1,
    Reversal = 2,
    Adjustment = 3
}

public enum ReviewCaseStatus
{
    Open = 1,
    Investigating = 2,
    Resolved = 3,
    Dismissed = 4
}

public enum PayoutStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Held = 4
}
