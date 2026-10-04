#pragma warning disable CA1825, CA1861
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Relay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFrontendFieldCoverage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudienceDescription",
                table: "communities",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "CommunityLink", table: "communities", type: "character varying(2048)", maxLength: 2048, nullable: true);
            migrationBuilder.AddColumn<string>(name: "VerificationEvidenceKey", table: "communities", type: "character varying(512)", maxLength: 512, nullable: true);
            migrationBuilder.AddColumn<string>(name: "AdvertiserName", table: "campaigns", type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<int>(name: "DurationDays", table: "campaigns", nullable: false, defaultValue: 7);
            migrationBuilder.AddColumn<int>(name: "MaximumAudience", table: "campaigns", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "MinimumAudience", table: "campaigns", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<string[]>(name: "Platforms", table: "campaigns", type: "text[]", nullable: false, defaultValue: Array.Empty<string>());
            migrationBuilder.Sql("UPDATE campaigns SET \"StartDate\" = DATE '2025-01-01' WHERE \"StartDate\" IS NULL;");
            migrationBuilder.Sql("UPDATE campaigns SET \"EndDate\" = DATE '2025-01-08' WHERE \"EndDate\" IS NULL;");
            migrationBuilder.AlterColumn<DateOnly>(name: "StartDate", table: "campaigns", type: "date", nullable: false, defaultValue: new DateOnly(2025, 1, 1), oldClrType: typeof(DateOnly), oldType: "date", oldNullable: true);
            migrationBuilder.AlterColumn<DateOnly>(name: "EndDate", table: "campaigns", type: "date", nullable: false, defaultValue: new DateOnly(2025, 1, 8), oldClrType: typeof(DateOnly), oldType: "date", oldNullable: true);

            migrationBuilder.CreateTable(name: "user_profiles", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                Industry = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                Website = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                Location = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                PrimaryGoal = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                PhoneNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                AvatarKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                OnboardingCompleted = table.Column<bool>(type: "boolean", nullable: false),
                CommunityName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                CommunityPlatform = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                CommunityMembers = table.Column<int>(type: "integer", nullable: true),
                CommunityCategory = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true)
            }, constraints: table => table.PrimaryKey("PK_user_profiles", x => x.Id));
            migrationBuilder.CreateIndex(name: "IX_user_profiles_UserId", table: "user_profiles", column: "UserId", unique: true);

            migrationBuilder.CreateTable(name: "admin_notes", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                EntityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                AuthorId = table.Column<Guid>(type: "uuid", nullable: false)
            }, constraints: table => table.PrimaryKey("PK_admin_notes", x => x.Id));
            migrationBuilder.CreateIndex(name: "IX_admin_notes_EntityType_EntityId_CreatedAt", table: "admin_notes", columns: new[] { "EntityType", "EntityId", "CreatedAt" });

            migrationBuilder.CreateTable(name: "review_cases", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                EntityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Reason = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                ResolutionAction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                ReportedBy = table.Column<Guid>(type: "uuid", nullable: true),
                ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            }, constraints: table => table.PrimaryKey("PK_review_cases", x => x.Id));
            migrationBuilder.CreateIndex(name: "IX_review_cases_Status_CreatedAt", table: "review_cases", columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateTable(name: "payout_records", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CommunityOwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                PlacementId = table.Column<Guid>(type: "uuid", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
            }, constraints: table => table.PrimaryKey("PK_payout_records", x => x.Id));
            migrationBuilder.CreateIndex(name: "IX_payout_records_CommunityOwnerId_Status_CreatedAt", table: "payout_records", columns: new[] { "CommunityOwnerId", "Status", "CreatedAt" });
            migrationBuilder.CreateIndex(name: "IX_payout_records_PlacementId", table: "payout_records", column: "PlacementId", unique: true);

            migrationBuilder.CreateTable(name: "admin_notifications", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                Href = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            }, constraints: table => table.PrimaryKey("PK_admin_notifications", x => x.Id));
            migrationBuilder.CreateIndex(name: "IX_admin_notifications_ReadAt_CreatedAt", table: "admin_notifications", columns: new[] { "ReadAt", "CreatedAt" });

            migrationBuilder.CreateTable(name: "admin_settings", columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Value = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false)
            }, constraints: table => table.PrimaryKey("PK_admin_settings", x => x.Id));
            migrationBuilder.CreateIndex(name: "IX_admin_settings_Key", table: "admin_settings", column: "Key", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "admin_notes"); migrationBuilder.DropTable(name: "review_cases"); migrationBuilder.DropTable(name: "payout_records"); migrationBuilder.DropTable(name: "admin_notifications"); migrationBuilder.DropTable(name: "admin_settings"); migrationBuilder.DropTable(name: "user_profiles");
            migrationBuilder.DropColumn(name: "AdvertiserName", table: "campaigns"); migrationBuilder.DropColumn(name: "DurationDays", table: "campaigns"); migrationBuilder.DropColumn(name: "MaximumAudience", table: "campaigns"); migrationBuilder.DropColumn(name: "MinimumAudience", table: "campaigns"); migrationBuilder.DropColumn(name: "Platforms", table: "campaigns"); migrationBuilder.DropColumn(name: "StartDate", table: "campaigns"); migrationBuilder.DropColumn(name: "EndDate", table: "campaigns"); migrationBuilder.DropColumn(name: "AudienceDescription", table: "communities"); migrationBuilder.DropColumn(name: "CommunityLink", table: "communities"); migrationBuilder.DropColumn(name: "VerificationEvidenceKey", table: "communities");
        }
    }
}
