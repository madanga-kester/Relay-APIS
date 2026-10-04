# Frontend integration backlog

These items were identified while connecting the current React application to the C# API. They are intentionally recorded rather than silently omitted.

## Authentication and identity

- Login and registration now call the Relay cookie endpoints when backend mode is enabled, preserve the selected role, and validate the backend's 12-character password requirement. Full session-provider replacement for all legacy routes is still pending.
- A migration/role-linking flow is needed if existing frontend users should be imported into PostgreSQL without creating duplicate accounts.

## Identifier and data migration

- Existing LocalStorage campaign, community, application, and placement IDs are human-readable strings. The backend uses UUIDs. A one-time import/mapping table is needed to connect existing browser records to PostgreSQL records.
- Existing seeded marketplace records are still rendered from LocalStorage; backend bootstrap/seed or import should be implemented before replacing local reads.
- Authenticated read endpoints now exist for advertiser-owned campaigns (`/campaigns/mine`), Community Owner-owned communities (`/communities/mine`), current applications (`/applications/mine`), and participant placements (`/placements/mine`). The frontend session gate and Campaign Owner campaign hydration use them when Relay mode is enabled; legacy records remain fallback data until UUID migration.
- Community Owner application and Accepted Campaign views now hydrate status, placement status, and tracking IDs from `/applications/mine`; Campaign Owner placement list/detail views hydrate from `/placements/mine`. Newly added local communities are now included in shared community reads without removing seeded communities.

## Applications and placements

- Community Owner application submission retains LocalStorage continuity and calls the backend once when campaign/community UUID mappings are available; the duplicate submission path has been removed.
- Campaign Owner accept/reject now uses the backend first for UUID-backed applications, updates LocalStorage only after confirmation, and retains legacy mock IDs locally.
- Backend application acceptance currently needs explicit placement creation/mapping validation for every imported legacy application.
- Community Owner Ready-to-Post → Active confirmation now uses the backend first when a backend placement ID is available, with safe legacy fallback.
- Placement list/detail views now hydrate status and tracking IDs from the backend when mappings exist, while retaining LocalStorage-derived financial display data during migration.

## Campaign lifecycle

- Campaign Owner publish/pause/resume/end actions now use the mapped backend campaign UUID when available, while retaining LocalStorage fallback for legacy records.
- Campaign editing now has an authenticated `PUT /api/v1/campaigns/{id}` endpoint. Only Draft campaigns can be edited; ownership, validation, and immutable post-publication financial-history guards are enforced.

## Admin workspace

- Admin settings, user status changes, campaign moderation, community moderation, application review, placement status changes, Admin profile edits, notes, payouts, notifications, and bulk actions now call protected Relay endpoints when UUID mappings exist; legacy LocalStorage remains a safe fallback during migration.
- The Admin Dashboard now reads authoritative user, campaign, community, application, placement, click, spend, earnings, and platform-revenue KPIs from `GET /api/v1/admin/overview` when Relay backend mode and a valid Admin cookie are present; it falls back to LocalStorage during migration.
- Reports, platform health, and financial/reconciliation views now use protected server-side aggregate endpoints with browser-derived fallback values.
- Admin notification list/read, review-case, note, payout, settings, and status-operation endpoints are available with pagination contracts.

## Files and external services

- Community verification evidence uses a secure object-storage upload endpoint and Admin-only short-lived review URLs; expired/unavailable evidence falls back to an explicit unavailable state in the UI.
- Password reset screens now call Relay forgot-password and reset-password endpoints when backend mode is enabled; token, expiry, and server error states are surfaced in the UI.

## Safe current behavior

The connected flows use the Relay API when `VITE_RELAY_API_URL` or `VITE_RELAY_BACKEND_ENABLED=true` is configured and the user has a valid Relay cookie session. If the API is unavailable, existing LocalStorage behavior remains available so the current frontend does not break during staged migration.
