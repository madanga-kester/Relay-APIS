using Microsoft.EntityFrameworkCore;
using Relay.Domain.Entities;
using Relay.Domain.Enums;

namespace Relay.Infrastructure.Persistence;

public sealed class RelayDbContext(DbContextOptions<RelayDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Community> Communities => Set<Community>();
    public DbSet<CampaignApplication> Applications => Set<CampaignApplication>();
    public DbSet<Placement> Placements => Set<Placement>();
    public DbSet<ClickEvent> ClickEvents => Set<ClickEvent>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<ActivityEvent> ActivityEvents => Set<ActivityEvent>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<AdminNote> AdminNotes => Set<AdminNote>();
    public DbSet<ReviewCase> ReviewCases => Set<ReviewCase>();
    public DbSet<PayoutRecord> PayoutRecords => Set<PayoutRecord>();
    public DbSet<AdminNotification> AdminNotifications => Set<AdminNotification>();
    public DbSet<AdminSetting> AdminSettings => Set<AdminSetting>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();


    
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureCampaigns(modelBuilder);
        ConfigureCommunities(modelBuilder);
        ConfigureApplications(modelBuilder);
        ConfigureVerificationCodes(modelBuilder);
        ConfigurePlacements(modelBuilder);
        ConfigureClicks(modelBuilder);
        ConfigureLedger(modelBuilder);
        ConfigureActivity(modelBuilder);
        ConfigureResetTokens(modelBuilder);
        ConfigureNotifications(modelBuilder);
        ConfigureUserPreferences(modelBuilder);
        modelBuilder.ConfigureProfilesAndAdminRecords();
    }

    private static void ConfigureUsers(ModelBuilder builder)
    {
        var entity = builder.Entity<UserAccount>();
        entity.ToTable("user_accounts");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
        entity.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
        entity.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
        entity.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        entity.HasIndex(x => x.NormalizedEmail).IsUnique();
        entity.HasIndex(x => new { x.Role, x.Status });
        entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
    }

    private static void ConfigureCampaigns(ModelBuilder builder)
    {
        var entity = builder.Entity<Campaign>();
        entity.ToTable("campaigns", table =>
        {
            table.HasCheckConstraint("ck_campaign_cpc_positive", "\"Cpc\" > 0");
            table.HasCheckConstraint("ck_campaign_budget_positive", "\"Budget\" > 0");
            table.HasCheckConstraint("ck_campaign_max_communities_positive", "\"MaximumCommunities\" > 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        entity.Property(x => x.AdvertiserName).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        entity.Property(x => x.Advertisement).HasMaxLength(4000).IsRequired();
        entity.Property(x => x.DestinationUrl).HasMaxLength(2048).IsRequired();
        entity.Property(x => x.Category).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Location).HasMaxLength(180).IsRequired();
        entity.Property(x => x.Cpc).HasPrecision(18, 2);
        entity.Property(x => x.Budget).HasPrecision(18, 2);
        entity.Property(x => x.Platforms).HasColumnType("text[]").IsRequired();
        entity.Property(x => x.MinimumAudience).IsRequired();
        entity.Property(x => x.MaximumAudience).IsRequired();
        entity.Property(x => x.DurationDays).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.HasIndex(x => new { x.AdvertiserId, x.Status });
        entity.HasIndex(x => new { x.Status, x.CreatedAt });
    }

    private static void ConfigureCommunities(ModelBuilder builder)
    {
        var entity = builder.Entity<Community>();
        entity.ToTable("communities", table => table.HasCheckConstraint("ck_community_members_positive", "\"Members\" > 0"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        entity.Property(x => x.Category).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Location).HasMaxLength(180).IsRequired();
        entity.Property(x => x.CommunityLink).HasMaxLength(2048);
        entity.Property(x => x.AudienceDescription).HasMaxLength(4000).IsRequired();
        entity.Property(x => x.VerificationEvidenceKey).HasMaxLength(512);
        entity.Property(x => x.Platform).HasConversion<string>().HasMaxLength(32);
        entity.Property(x => x.VerificationStatus).HasConversion<string>().HasMaxLength(32);
        entity.HasIndex(x => new { x.OwnerId, x.VerificationStatus });
    }

    private static void ConfigureApplications(ModelBuilder builder)
    {
        var entity = builder.Entity<CampaignApplication>();
        entity.ToTable("campaign_applications");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Cpc).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.HasIndex(x => new { x.CampaignId, x.CommunityId }).IsUnique();
        entity.HasIndex(x => new { x.CommunityOwnerId, x.Status });
        entity.HasOne(x => x.Placement).WithOne().HasForeignKey<Placement>(x => x.ApplicationId).OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurePlacements(ModelBuilder builder)
    {
        var entity = builder.Entity<Placement>();
        entity.ToTable("placements");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TrackingId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.HasIndex(x => x.TrackingId).IsUnique();
        entity.HasIndex(x => new { x.CampaignId, x.Status });
        entity.HasIndex(x => new { x.CommunityOwnerId, x.Status });
    }

    private static void ConfigureClicks(ModelBuilder builder)
    {
        var entity = builder.Entity<ClickEvent>();
        entity.ToTable("click_events");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ClickId).HasMaxLength(160).IsRequired();
        entity.Property(x => x.TrackingId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.IdempotencyKey).HasMaxLength(160).IsRequired();
        entity.Property(x => x.RejectionReason).HasMaxLength(240);
        entity.Property(x => x.VisitorKeyHash).HasMaxLength(128);
        entity.Property(x => x.Qualification).HasConversion<string>().HasMaxLength(32);
        entity.HasIndex(x => x.ClickId).IsUnique();
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => new { x.CampaignId, x.Qualification, x.CreatedAt });
        entity.HasIndex(x => new { x.TrackingId, x.CreatedAt });
    }

    private static void ConfigureLedger(ModelBuilder builder)
    {
        var entity = builder.Entity<LedgerEntry>();
        entity.ToTable("ledger_entries", table => table.HasCheckConstraint("ck_ledger_balanced", "\"AdvertiserCharge\" = \"CommunityOwnerEarning\" + \"PlatformFee\""));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        entity.Property(x => x.AdvertiserCharge).HasPrecision(18, 2);
        entity.Property(x => x.CommunityOwnerEarning).HasPrecision(18, 2);
        entity.Property(x => x.PlatformFee).HasPrecision(18, 2);
        entity.HasIndex(x => x.ClickEventId).IsUnique();
        entity.HasIndex(x => new { x.CampaignId, x.CreatedAt });
    }

    private static void ConfigureActivity(ModelBuilder builder)
    {
        var entity = builder.Entity<ActivityEvent>();
        entity.ToTable("activity_events");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        entity.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        entity.Property(x => x.EntityName).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Detail).HasMaxLength(2000).IsRequired();
        entity.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt });
        entity.HasIndex(x => new { x.EventType, x.CreatedAt });
    }

    private static void ConfigureResetTokens(ModelBuilder builder)
    {
        var entity = builder.Entity<PasswordResetToken>();
        entity.ToTable("password_reset_tokens");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        entity.HasIndex(x => x.TokenHash).IsUnique();
        entity.HasIndex(x => new { x.UserId, x.ExpiresAt });
    }


    private static void ConfigureVerificationCodes(ModelBuilder builder)
    {
        var entity = builder.Entity<EmailVerificationCode>();
        entity.ToTable("email_verification_codes");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CodeHash).HasMaxLength(128).IsRequired();
        entity.HasIndex(x => x.UserId);
    }



    private static void ConfigureNotifications(ModelBuilder builder)
    {
        var entity = builder.Entity<UserNotification>();
        entity.ToTable("user_notifications");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Type).HasMaxLength(32).IsRequired();
        entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Body).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.Href).HasMaxLength(200);
        entity.HasIndex(x => new { x.UserId, x.CreatedAt });
    }

    private static void ConfigureUserPreferences(ModelBuilder builder)
    {
        var entity = builder.Entity<UserPreference>();
        entity.ToTable("user_preferences");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ValuesJson).HasMaxLength(8000).IsRequired();
        entity.HasIndex(x => x.UserId).IsUnique();
    }

}
