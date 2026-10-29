## Read-only retail administration foundation

Adds a separate .NET 10 MVC/Razor workspace for catalog, inventory and order oversight through the existing MerchantOS POS REST API. The dashboard identifies sales as the latest-100-order window; unavailable upstream data is displayed honestly. No POS changes, direct database access or administrative writes are introduced.

Bootstrap provides the responsive navigation/forms/tables, jQuery enhances filtering, and a precompiled Handlebars template safely renders dynamic rows. Standard GET forms and server-rendered details work without JavaScript.

### Validation

- Local Release build: zero warnings/errors.
- 26 passing .NET tests, 3 passing template checks.
- 15 passing desktop/tablet/mobile browser checks against an explicit contract fixture; this does not prove PostgreSQL integration.
- Required before ready-for-review: successful pinned POS/PostgreSQL 17 GitHub Actions run, real integration verification and uploaded screenshots/reports.

Security: local/isolated demo only; both applications require an authorization strategy before public use or future admin writes. Do not merge automatically.
