## Verify the read-only MVC admin foundation

The initial user upload already contains the .NET 10 MVC/Razor application. This PR pins CI to Playwright-supported Ubuntu 24.04, replaces contract-fixture README screenshots with real database-backed captures, and records the verified implementation and remaining limitations. Main is preserved and MerchantOS POS is unchanged.

The application provides overview, catalog/product details, inventory and order/receipt pages through typed server-side POS API integration. Bootstrap supplies responsive navigation/forms/tables; jQuery and escaped precompiled Handlebars enhance catalog filtering while Razor GET forms remain usable without JavaScript. There is no second database, checkout implementation or admin write surface.

### Validation

[Successful integration run](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/40957510020) · [Screenshots, HTML/TRX reports and logs](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/40957510020/artifacts/12933623920)

- .NET Release builds: zero warnings/errors.
- 26 .NET tests, 3 template checks, 15 desktop/tablet/mobile Playwright tests pass with zero retries.
- Real pinned POS API + PostgreSQL 17: seed/migration, health/catalog contracts, checkout/idempotency/concurrency, 30 product/inventory records, 5 persisted receipts and four main Razor pages verified.
- Local dependency audit: zero vulnerabilities.

### Boundaries

Local/isolated demo only. Authentication/authorization and safe backend contracts must precede future writes or public use. Sales metrics and order search cover the latest 100 orders, not lifetime reporting. No public deployment. Leave this PR unmerged.
