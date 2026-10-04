using Microsoft.EntityFrameworkCore;
using Relay.Domain.Entities;

namespace Relay.Infrastructure.Persistence;

public static class AdminRecordModelConfiguration
{
    public static void ConfigureProfilesAndAdminRecords(this ModelBuilder builder)
    {
        var profile = builder.Entity<UserProfile>();
        profile.ToTable("user_profiles"); profile.HasKey(x => x.Id); profile.HasIndex(x => x.UserId).IsUnique();
        profile.Property(x => x.BusinessName).HasMaxLength(200); profile.Property(x => x.Industry).HasMaxLength(160); profile.Property(x => x.Website).HasMaxLength(2048); profile.Property(x => x.Location).HasMaxLength(180); profile.Property(x => x.PrimaryGoal).HasMaxLength(240); profile.Property(x => x.PhoneNumber).HasMaxLength(40); profile.Property(x => x.AvatarKey).HasMaxLength(512); profile.Property(x => x.CommunityName).HasMaxLength(160); profile.Property(x => x.CommunityPlatform).HasMaxLength(40); profile.Property(x => x.CommunityCategory).HasMaxLength(120);

        var note = builder.Entity<AdminNote>();
        note.ToTable("admin_notes"); note.HasKey(x => x.Id); note.Property(x => x.EntityType).HasMaxLength(64).IsRequired(); note.Property(x => x.EntityName).HasMaxLength(200).IsRequired(); note.Property(x => x.Text).HasMaxLength(4000).IsRequired(); note.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt });
        var review = builder.Entity<ReviewCase>();
        review.ToTable("review_cases"); review.HasKey(x => x.Id); review.Property(x => x.EntityType).HasMaxLength(64).IsRequired(); review.Property(x => x.EntityName).HasMaxLength(200).IsRequired(); review.Property(x => x.Reason).HasMaxLength(4000).IsRequired(); review.Property(x => x.ResolutionAction).HasMaxLength(2000); review.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); review.HasIndex(x => new { x.Status, x.CreatedAt });
        var payout = builder.Entity<PayoutRecord>();
        payout.ToTable("payout_records"); payout.HasKey(x => x.Id); payout.Property(x => x.Amount).HasPrecision(18, 2); payout.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); payout.Property(x => x.FailureReason).HasMaxLength(1000); payout.HasIndex(x => new { x.CommunityOwnerId, x.Status, x.CreatedAt }); payout.HasIndex(x => x.PlacementId).IsUnique();
        var notification = builder.Entity<AdminNotification>();
        notification.ToTable("admin_notifications"); notification.HasKey(x => x.Id); notification.Property(x => x.Type).HasMaxLength(80).IsRequired(); notification.Property(x => x.Title).HasMaxLength(200).IsRequired(); notification.Property(x => x.Description).HasMaxLength(2000).IsRequired(); notification.Property(x => x.Href).HasMaxLength(512).IsRequired(); notification.HasIndex(x => new { x.ReadAt, x.CreatedAt });
        var setting = builder.Entity<AdminSetting>();
        setting.ToTable("admin_settings"); setting.HasKey(x => x.Id); setting.Property(x => x.Key).HasMaxLength(120).IsRequired(); setting.Property(x => x.Value).HasMaxLength(10000).IsRequired(); setting.HasIndex(x => x.Key).IsUnique();
    }
}
