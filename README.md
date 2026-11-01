> **Milestone 8 review candidate:** mandatory Administrator MFA and secure account recovery. [Enrollment, recovery, security events and restore reconciliation](docs/mfa-recovery.md). Milestone 7 is merged. Executed M8 integration and HTTPS recovery evidence is recorded in [readiness](docs/staging-readiness.md); final-head checks govern review readiness. No merge or public deployment.

# MerchantOS Admin

ASP.NET Core MVC/Razor 10 retail administration, with Bootstrap 5, jQuery, escaped precompiled Handlebars and authenticated server-side POS API integration. The existing POS PostgreSQL database remains the only data authority. No separate Admin identity database or service-account password exists. Durable staging tickets use the existing POS database.

## Current capabilities

- Approved Overview, Products, Inventory, Orders and Order Details at 1440×900, 768×1024 and 390×844.
- Authenticated Administrator sessions, safe sign-in returns, CSRF-protected logout, per-request backend role/session validation.
- Product drawer: Edit Product, Adjust Stock and View History. Names (English/Arabic), category and integer-halalas selling price can change; product IDs/SKUs and historical receipts cannot.
- Restock, damage/loss and absolute stock corrections require a reason, a current stock version and explicit confirmation. Backend row locks and idempotency protect balances and retries.
- Bounded, persistent history shows authoritative actor, UTC time, before/after values and reason.
- Existing filters/sorting/seven-row pagination remain; rows and stock summaries refresh after confirmed writes. Orders now query complete history on the server. Stock threshold 5 remains a presentation rule.

[Management contracts and behavior](docs/management.md) · [Authentication and setup](docs/authentication.md) · [Executed status](PROJECT_STATUS.md).

## Run together locally

Check out the exact POS revision in `.github/workflows/verify.yml` (management PR #4 dependency). Apply every POS migration in numeric order to the existing database; never reset/reseed a populated store. Follow POS docs/authentication.md to configure a database connection and provision separate development Administrator/Cashier accounts using environment-only credentials.

Prefer HTTPS with trusted development certificates. For explicitly local HTTP only, set `ASPNETCORE_ENVIRONMENT=Development` and `Security__AllowLoopbackHttp=true` in both processes. Both Host and remote IP must remain loopback. Start the POS API at127.0.0.1:5080; set `PosApi__BaseUrl=http://127.0.0.1:5080/` for Admin.

```sh
npm ci
npm run assets
dotnet run --project src/MerchantOS.Admin/MerchantOS.Admin.csproj
```

Browse http://127.0.0.1:5090 and sign in as Administrator. Cashiers use Angular POS and cannot perform administrative writes. No known/default passwords exist. Application/browser secrets and storage state must stay outside source/artifacts.

## Verify

```sh
npm run test:assets
dotnet test tests/MerchantOS.Admin.Tests/MerchantOS.Admin.Tests.csproj -c Release
npx playwright install chromium
npm run test:e2e
```

Browser/integration verification requires the real POS and isolated PostgreSQL fixture with environment-provided temporary users; use the workflow for reproducibility. Management tests intentionally modify isolated data, then restore product/stock through audited APIs. Do not run them on valuable data. Authentication stays enabled. CI pins the POS commit, runs checkout/security/management integration, all existing responsive tests and new form scenarios, and audits reports for credential exposure before uploading.

Baseline security PR #5 and responsive PRs#1–4 are merged. Current Milestone 5 is [Admin PR #6](https://github.com/danishirfan21/MerchantOS-Admin/pull/6), dependent on [POS PR #4](https://github.com/danishirfan21/MerchantOS-POS/pull/4); both are merged. Historical visual decisions: [desktop](docs/desktop-visual-review.md), [tablet](docs/tablet-visual-review.md), [mobile](docs/mobile-visual-review.md), [assets](docs/design-assets.md).

## Limits

Local/isolated demo only; no public deployment approval. Staging uses durable encrypted tickets and protected persistent keys, with gated operator provisioning/revocation. Assigned recovery/operator custody, multi-instance/distributed throttling, off-host backup/key custody and operational monitoring remain public-deployment blockers. No creation/deletion, SKU reassignment, refunds, suppliers, user management or new payment integration. New forms require JavaScript; existing read-only GET pages continue working without it. Audit before/after fields is compact and bounded, not an enterprise audit dashboard. No production readiness claim is made from CI.

## Historical reporting (Milestone 6)

Overview uses a PostgreSQL reporting snapshot for Today/Yesterday/Last7Days/Last30Days/Custom Range in Asia/Riyadh. Sales revenue includes VAT; a compact breakdown explains gross, discounts, net-before-VAT, VAT and rounded average order. Catalog and inventory cards remain explicitly current snapshots. The accessible Sales Rhythm chart uses real zero-filled hourly/daily/monthly buckets. Payment amounts distinguish simulated card; top products use historical line prices before discounts/VAT.

Orders queries the server for every page/filter and browses complete history. Seven rows/page preserves the approved layout; the API defaults20 and bounds pages to100rows. CSV uses the exact selected filters and max10000rows. See [reporting architecture, formulas, date policy and limits](docs/reporting.md). Current feature [Admin PR #7](https://github.com/danishirfan21/MerchantOS-Admin/pull/7) depends on POS PR #5; both are merged. Milestone 5 PR #6 is already merged.


## Milestone 7 — operational staging

Milestone 7 is merged in both repositories. See [staging operations and incident runbook](docs/staging-operations.md) and [evidence/readiness matrix](docs/staging-readiness.md). Persistent protected keys, durable Admin tickets, strict local HTTPS Compose, safe migrations, encrypted backup/restore, health/readiness and correlation-safe logging are implemented. The real HTTPS Compose recovery drill passed 176 assertions; complete existing regression suites and credential audits pass. Remaining image findings and production blockers are recorded in the matrix. No public deployment or automatic merge.
