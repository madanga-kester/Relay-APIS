# Frontend field and flow coverage

The backend now has server-side contracts and persistence for the existing frontend flows without requiring a frontend rewrite.

## Campaign Owner

`CreateCampaignRequest` and `Campaign` persist campaign name, advertiser name, description, approved advertisement, destination URL, target platforms, minimum/maximum audience, category, location, duration, maximum communities, CPC, budget, start date, end date, and lifecycle status.

Campaign creation retains the existing budget guard and server-side ownership checks. Existing lifecycle endpoints remain authoritative for Draft, Published, Active, Paused, Completed, and Budget Exhausted.

## Community Owner

`CreateCommunityRequest` and `Community` persist community name, platform, member count, category, location, community link, audience description, and verification evidence object key. Verification remains server-controlled.

## Onboarding and profiles

`UserProfile` stores Advertiser/Community Owner onboarding fields (contact/business information, industry, website, location, primary goal, community name/platform/audience/category) plus editable phone, avatar key, and completion state. `GET/PUT /api/v1/profile` is authenticated and persists these values.

## Marketplace financial flow

Qualified clicks continue to use the shared financial calculator. Each qualified click creates one ledger entry and accumulates the Community Owner portion into a placement payout record. The existing idempotency key and campaign budget transaction remain in the click path.

## Admin Operations Center

The backend now stores internal notes, review/dispute cases, payout status, Admin notifications, and Admin settings. These endpoints require the server-side Admin role and are not exposed through ordinary user authorization.

- `api/v1/admin/notes`
- `api/v1/admin/reviews`
- `api/v1/admin/payouts`
- `api/v1/admin/notifications`
- `api/v1/admin/settings/{key}`

The frontend can adopt these endpoints incrementally; the current frontend-only LocalStorage behavior remains untouched in this backend-focused phase.
