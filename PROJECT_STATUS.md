# MerchantOS Admin — desktop iteration 1

Updated 30 October 2026 (Asia/Karachi).

**Implemented and verified at 1440 × 900.**

- Repository: https://github.com/danishirfan21/MerchantOS-Admin
- Feature branch: `feature/stitch-desktop`
- Desktop PR: https://github.com/danishirfan21/MerchantOS-Admin/pull/2
- Passing real integration run: https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41084505556
- Five desktop screenshots, receipt PDF, HTML/TRX reports and logs: https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41084505556/artifacts/12991545523

## Implemented desktop screens

| Screen | Working behavior |
| --- | --- |
| Overview | Four real KPI cards, latest-activity hourly chart, recent orders, low/depleted stock alerts |
| Products | Actual catalog, category/stock/search filters, integer-price sorting, seven-row pagination, read-only product drawer |
| Inventory | Stock summaries, attention banner, demo threshold, search/filters/pagination and product details |
| Orders | Actual latest-100-order window, order search, UTC date/payment filters, pagination, receipt navigation |
| Order Details | Persisted item/price/quantity and financial values, cash/simulated-card display, cash change, working print receipt |

Shared Razor layout/partials implement the supplied dark slate sidebar, compact header, typography, surfaces, cards, tables, badges and thumbnails. Bootstrap supplies offcanvas/components; jQuery and precompiled escaped Handlebars implement progressive search, paging and details. The three browsing screens have a working header product search and Ctrl+K focus shortcut. Local fonts/assets preserve the restrictive CSP without runtime CDN calls.

## Executed verification

Passing desktop implementation run at `7fbdfb6ab9b359cd511a348deb252f32f969b0ea`:

| Check | Executed result |
| --- | --- |
| MVC, test project and POS Release builds | Passed; 0 warnings / 0 errors |
| .NET tests | 33 passed, 0 failed, 0 skipped |
| Safe template tests | 3 passed, 0 failed |
| Desktop Playwright | 15 passed, 0 failed, zero retries; 1440 × 900 |
| PostgreSQL 17 schema, 30-product seed and checkout migration | Passed |
| Pinned POS catalog/checkout checks | Passed; cash/card, failures, idempotency, snapshots, inventory, receipts and concurrency |
| Admin live integration | Passed; all 30 catalog/detail/inventory records across pagination, 5 real receipts and 4 main page routes |
| Browser artifact upload | Passed; five page screenshots, print PDF, browser report, TRX and service logs |

The suite covers all five pages, sidebar/global search, category/search/stock filters, sorting, pagination, read-only drawer, order navigation/payment filtering, backend total display, print behavior, loading/error state and no-JavaScript fallback. Additional assertions preserve the Inventory URL after AJAX pagination. No database-backed claim is based on the preliminary local contract fixture.

## Visual verification

All five supplied Stitch HTML references and actual MVC pages were rendered in Chromium at 1440 × 900 and visually compared. Corrections tightened summary card heights, screen-specific margins, filter/table separation, low-stock rows, receipt header and financial summary. All five real CI captures were also compared with the references. Measurements: sidebar 256px, header 64px, primary headings 24px; no unexpected document-level horizontal overflow on any desktop page. The print PDF was rendered and inspected.

See [desktop visual review](docs/desktop-visual-review.md) for intentional data/capability differences. The screenshots are actual Razor pages backed by the pinned POS API and PostgreSQL, not mockups or Stitch demo records. This is a faithful visual translation with live-data adaptations, not a pixel-identical claim.

## Boundaries and known limits

- Desktop is the only acceptance target in this iteration. No approved tablet/mobile UI has been implemented or newly claimed as visually verified. Existing narrow-screen fallback code remains.
- The POS API is unauthenticated: keep both apps local or inside isolated CI. No public deployment or insecure demonstration login.
- No writes, price overrides, product editing, stock adjustments, refunds, customer profiles, email receipts, real card data, multi-store records or new database were added.
- Order browsing/date/payment filters and sales metrics cover only the latest 100 API records. Dates are explicitly UTC. Item counts absent from the order-list contract are shown as receipt links; there is no N+1 receipt fetch.
- Sales chart: nine hourly buckets on the latest completed order date. It does not pretend to be complete lifetime or today-only reporting.
- Reorder threshold is an Admin presentation rule of 5 for every product; it is not persisted/backend configurable. Low stock is 1–5, depleted is 0, healthy is 6+.
- Thumbnails use the provided illustrative Stitch assets only for matching products; remaining products use POS category SVGs. They are not API-provided product photography.

## Repository state and next iteration

The user-uploaded main commit remains intact. This branch descends from verified foundation PR #1 (also unmerged) and targets main as requested. The prior foundation's complete run is https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/40958190772. Neither PR is merged automatically. MerchantOS POS source remains unchanged.

Next: supply the approved tablet screens for a separate implementation iteration, followed by the mobile designs. Administrative authentication and safe write contracts remain future milestone work requiring explicit scope.
