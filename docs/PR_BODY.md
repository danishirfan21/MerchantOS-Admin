## Implement the five approved Stitch desktop screens

Translates the supplied Overview, Products, Inventory, Orders and Order Details screens into a shared Razor/Bootstrap desktop UI at 1440 × 900. Preserves the verified POS REST integration and read-only boundaries.

Adds real-data KPI/hourly chart display, seven-row pagination, numeric catalog sorting, jQuery/Handlebars filtering and paging, a read-only Bootstrap product drawer, working header product search, UTC order filters, and print-friendly receipts. Fonts, icons and supplied illustrative thumbnails are local assets; no runtime Tailwind/CDN dependency.

### Validation

[Passing real integration run](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41084505556) · [Five page screenshots, print PDF, reports and logs](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41084505556/artifacts/12991545523)

- Release builds: 0 warnings/errors.
- 33 .NET tests, 3 template checks and 15 desktop Playwright checks pass with zero retries.
- Actual pinned POS API + PostgreSQL 17: seed/migration, catalog/checkout/idempotency/concurrency, all 30 inventory/product records, five receipts and four main page routes verified.
- All five actual desktop pages compared against the supplied Stitch HTML; layout corrections applied. No unexpected horizontal overflow. Print PDF rendered and inspected.

### Intentional boundaries

No tablet/mobile redesign, admin writes, POS changes, alternate database or public deployment. Static example metrics/customer/payment/multi-store data are not presented as live. Window/date/threshold limitations remain visible. See desktop visual-review documentation for the deliberate adaptations.

This branch includes the verified foundation ancestry from unmerged PR #1 and targets main as requested. Leave both PRs unmerged. Next UI iteration is the separately supplied tablet design.
