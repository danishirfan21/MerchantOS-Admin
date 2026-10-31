# MerchantOS Admin

ASP.NET Core MVC/Razor 10 retail administration, with Bootstrap 5, jQuery, escaped precompiled Handlebars and authenticated server-side POS API integration. The existing POS PostgreSQL database remains the only data authority. No Admin database or service-account password exists.

## Current capabilities

- Approved Overview, Products, Inventory, Orders and Order Details at 1440×900, 768×1024 and 390×844.
- Authenticated Administrator sessions, safe sign-in returns, CSRF-protected logout, per-request backend role/session validation.
- Product drawer: Edit Product, Adjust Stock and View History. Names (English/Arabic), category and integer-halalas selling price can change; product IDs/SKUs and historical receipts cannot.
- Restock, damage/loss and absolute stock corrections require a reason, a current stock version and explicit confirmation. Backend row locks and idempotency protect balances and retries.
- Bounded, persistent history shows authoritative actor, UTC time, before/after values and reason.
- Existing filters/sorting/seven-row pagination remain; rows and stock summaries refresh after confirmed writes. Orders still cover only the latest 100 receipts. Stock threshold 5 remains a presentation rule.

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

Baseline security PR #5 and responsive PRs#1–4 are merged. Current Milestone 5 is [Admin PR #6](https://github.com/danishirfan21/MerchantOS-Admin/pull/6), dependent on [POS PR #4](https://github.com/danishirfan21/MerchantOS-POS/pull/4); both remain unmerged for review. Historical visual decisions: [desktop](docs/desktop-visual-review.md), [tablet](docs/tablet-visual-review.md), [mobile](docs/mobile-visual-review.md), [assets](docs/design-assets.md).

## Limits

Local/isolated demo only; no public deployment approval. Single-instance Admin ticket cache, managed encrypted keys, MFA/recovery, production provisioning, distributed throttling/session storage and monitoring remain documented future work. No creation/deletion, SKU reassignment, refunds, suppliers, user management or new payment integration. New forms require JavaScript; existing read-only GET pages continue working without it. Audit before/after JSON is compact and bounded, not an enterprise audit dashboard. No production readiness claim is made from CI.
