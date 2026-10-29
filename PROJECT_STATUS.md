# MerchantOS Admin — project status

## Milestone 1: verified, pull request open

Updated 30 October 2026 (Asia/Karachi).

Repository: https://github.com/danishirfan21/MerchantOS-Admin
Feature branch: `feature/milestone-1-verification`
Pull request: https://github.com/danishirfan21/MerchantOS-Admin/pull/1
Verified integration run: https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/40957510020
Artifacts: https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/40957510020/artifacts/12933623920

The user uploaded the previously verified source as the initial main commit `e1add784b7f7540a02417efb58ceeeb1ae26bbef`. This feature PR preserves that upload and finalizes the CI runner, real integration evidence, screenshots and documentation. The original local implementation branch was `feature/milestone-1`.

## Implemented functionality

- .NET 10 / ASP.NET Core MVC, typed controllers, Razor Views, ViewModels, DI and asynchronous HttpClient integration.
- Overview dashboard, searchable/filterable product catalog, product details, inventory overview, recent order history and persisted receipt details.
- Bootstrap responsive sidebar/offcanvas navigation, forms, tables and restrained emerald/slate styling.
- Genuine jQuery AJAX catalog filtering and precompiled, HTML-escaped Handlebars rows, plus standard Razor GET-form fallback.
- Loading, empty, error/retry states; independently handled catalog/order failures; honest latest-100-order sales window.
- Existing POS REST API is the only data source. No duplicate catalog/database, direct SQL, checkout logic, administrative writes or public deployment.

## Executed verification

Passing run at implementation/runner commit `f0d0b42ddc01b624c3d56f27af57c3d56c3b5050`:

| Check | Result | Actual scope |
| --- | --- | --- |
| .NET restore and Release builds | Passed, 0 warnings / 0 errors | MVC, test project and pinned POS API |
| .NET tests | 26 passed / 0 failed / 0 skipped | Controllers, mappings, filtering, aggregation, API errors, empty data, Razor rendering and output encoding |
| Template checks | 3 passed / 0 failed | Dynamic HTML escaping, empty state and compiled runtime |
| PostgreSQL initialization | Passed | PostgreSQL 17, 30 seeded products/inventory records and checkout migration |
| Live POS catalog checks | Passed | Health, catalog, categories, search/filter, product/inventory detail and 404s |
| Live POS checkout checks | Passed | Cash/card, failures, idempotency, historical snapshots, inventory, receipts and concurrency |
| Admin live integration | Passed | 30 product/detail/inventory records, 5 real persisted order receipts, 4 main Razor pages |
| Playwright Chromium | 15 passed / 0 failed, zero retries | Desktop, tablet and mobile against the real POS API/PostgreSQL; includes filters, details, responsive navigation and no-JavaScript forms |
| Artifact upload | Passed | Screenshots, Playwright HTML report, xUnit TRX and service logs |
| Local npm audit | Passed, 0 vulnerabilities | Locked dependencies, including patched Handlebars 4.7.10 |

Real CI screenshots replace the earlier contract-fixture README captures. Desktop overview and mobile inventory captures were visually inspected. Local contract-fixture browser results were only preliminary checks; they are not the basis of the database-backed verification above.

## CI correction

The initial workflow reached live integration successfully but could not install the pinned Playwright browser on Ubuntu 26.04 after `ubuntu-latest` advanced. The workflow now pins `ubuntu-24.04`, and the complete integration/browser run succeeds. No POS source changes were needed.

## Known limitations

- The POS order endpoint exposes only its latest 100 orders. Completed-order counts, sales sums and order search apply to that window, with all dates. No lifetime totals or date-range reporting are implied.
- Inventory is a current catalog snapshot, not one transactional snapshot shared across independent API calls. Low stock is a presentation threshold of 1–5 units; zero is a separate status.
- The UI is English; Arabic product names are retained and searchable.
- Both applications are local/isolated demos. The upstream API is unauthenticated. No admin authentication, authorization, writes, store settings or real card processing are implemented.
- The feature PR remains unmerged. The POS repository remains unchanged; neither application was deployed publicly.

## Recommended next milestone

Milestone 2: design coordinated authenticated/authorized backend contracts for product editing and inventory adjustments, with validation and audit records. Obtain explicit authorization for any future POS changes; do not bypass checkout/inventory safeguards.
