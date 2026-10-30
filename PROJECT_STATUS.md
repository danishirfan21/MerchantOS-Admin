# MerchantOS Admin — tablet iteration 2

Updated 31 October 2026 (Asia/Karachi).

Branch: `feature/stitch-tablet`, based on merged desktop main `fa9f81d66b0c187d708e680b55db9aef5f6266b5`. Foundation PR #1 and desktop PR #2 are merged. Main post-merge verification [41111366791](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41111366791) passed.

## Implemented

All five supplied tablet sources inspected and rendered at 768 × 1024. Reuses existing Razor pages, partials, escaped Handlebars, GET-only services and Bootstrap components. Adds compact header, accessible navigation drawer, two-column dashboard/inventory cards, three catalog summaries, compact tables and stacked receipt sections. The same print button appears after Payment Method at tablet width. Desktop remains at 1440 × 900.

## Verification

- Local MVC Release build: passed, zero warnings/errors.
- Local .NET tests: 33 passed, zero failed/skipped.
- Local template checks: 3 passed, zero failed.
- Local tablet presentation checks: 10 passed, zero failed, using the explicitly documented contract fixture only. These do not verify the real API/database.
- Full real POS/PostgreSQL integration: passed; 30 catalog/detail/inventory records, 10 persisted receipts and four main routes. PostgreSQL 17 initialization/migrations and upstream checkout/concurrency checks passed.
- Desktop Chromium: 15 passed at 1440 × 900, zero failures/retries.
- Tablet Chromium: 19 passed at 768 × 1024, zero failures/retries.
- [Passing implementation run 41113661480](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41113661480), snapshot `56f8c1911a861dc14e460f1d53e0a081c4310a64`.
- [Actual desktop/tablet screenshots, full tablet pages, drawer capture, both print PDFs, browser report, TRX and logs](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41113661480/artifacts/13005990377).
- All five desktop pages were pixel-identical to pristine main under identical local fixture data. This is presentation evidence only; the CI above uses the real database.

The final refinement commit tightens tablet-only catalog card spacing/type and amber low-stock color and records this evidence. Current-head CI and final artifact links are linked in [tablet PR #3](https://github.com/danishirfan21/MerchantOS-Admin/pull/3), which remains unmerged.

The browser suite retains every desktop assertion and adds touch/drawer focus and close checks, two-column/stacking assertions, compact table checks, order search/date/pagination, control touch targets, table overflow, and one final receipt print action. Retries remain zero.

## Scope and adaptations

Mobile iteration 3, POS changes, writes, authentication and public deployment remain deferred. The latest-100-order window, UTC date filters and uniform demo stock threshold (5 units) remain explicit. Stitch's illustrative metrics, terminal/customer/compliance data and unsupported notifications/settings are not presented as actual data. Seven rows per page are retained across devices. Tablet catalog summary shows actual catalog/healthy/attention counts rather than an invented catalog valuation.

See [tablet visual review](docs/tablet-visual-review.md). Historical desktop verification follows.

---

# Desktop iteration 1 verification (historical)

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
