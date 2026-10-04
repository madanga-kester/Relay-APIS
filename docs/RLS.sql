-- Apply after the initial EF migration and after the application has been verified against PostgreSQL.
-- The API still enforces ownership and role checks; RLS is defense in depth, not a replacement.

ALTER TABLE campaigns ENABLE ROW LEVEL SECURITY;
ALTER TABLE communities ENABLE ROW LEVEL SECURITY;
ALTER TABLE campaign_applications ENABLE ROW LEVEL SECURITY;
ALTER TABLE placements ENABLE ROW LEVEL SECURITY;
ALTER TABLE click_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE ledger_entries ENABLE ROW LEVEL SECURITY;

-- The application connection role should set these transaction-local settings after authentication:
-- SET LOCAL app.user_id = '<uuid>';
-- SET LOCAL app.user_role = 'Advertiser' | 'CommunityOwner' | 'Admin';

CREATE POLICY campaigns_owner_or_public ON campaigns
  USING (current_setting('app.user_role', true) = 'Admin'
      OR "AdvertiserId"::text = current_setting('app.user_id', true)
      OR "Status" IN ('Published', 'Active'));

CREATE POLICY communities_owner_or_verified ON communities
  USING (current_setting('app.user_role', true) = 'Admin'
      OR "OwnerId"::text = current_setting('app.user_id', true)
      OR "VerificationStatus" = 'Verified');

CREATE POLICY applications_participant_or_admin ON campaign_applications
  USING (current_setting('app.user_role', true) = 'Admin'
      OR "CommunityOwnerId"::text = current_setting('app.user_id', true)
      OR EXISTS (SELECT 1 FROM campaigns c WHERE c."Id" = "CampaignId" AND c."AdvertiserId"::text = current_setting('app.user_id', true)));

CREATE POLICY placements_participant_or_admin ON placements
  USING (current_setting('app.user_role', true) = 'Admin'
      OR "CommunityOwnerId"::text = current_setting('app.user_id', true)
      OR EXISTS (SELECT 1 FROM campaigns c WHERE c."Id" = "CampaignId" AND c."AdvertiserId"::text = current_setting('app.user_id', true)));

-- Click and ledger writes should be performed by the tracking service through a tightly scoped DB role.
-- Add explicit INSERT/SELECT policies for that role after the deployment identity model is finalized.
