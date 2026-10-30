# Authentication integration milestone

Branch `feature/auth-integration`, based on responsive main `64b278a180083998c6888a5d1effc83dd85cbd7c`. Depends on POS security PR #3 and its exact pinned revision. Native server-managed Admin sessions, role validation on each request, secure login/logout, safe local returns, CSRF, pooled-client isolation and identity indicators implemented. Original business views/calculations preserved. Local .NET tests: 39 passed (33 preserved + six HTTP security tests). [Verified implementation run](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41138596485), head `76f7fc4aa3e843c0b29f3a5b0edcf7e4c0c78b86`, passed 39 Admin .NET + 9 POS .NET tests, 3 template checks and 61 browser tests (49 preserved functional + 12 authentication across desktop/tablet/mobile), with zero retries. Actual POS/PostgreSQL initialization, migration, authenticated catalog/checkout/idempotency/concurrency, 30 catalog/detail/inventory records, ten receipts and 38 direct security/log checks passed. [Screenshots, login views, PDFs, reports and logs](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41138596485/artifacts/13017426719).

Final CI pins POS `99cfdf755f55fbfac9f8c1f305815f730a489d3a`, including a populated-data migration replay check. Final-head results are linked in [Admin PR #5](https://github.com/danishirfan21/MerchantOS-Admin/pull/5). Both this PR and [POS PR #3](https://github.com/danishirfan21/MerchantOS-POS/pull/3) remain unmerged. The necessary user indicator/Sign out controls are the only changes to existing business-screen markup; there is no dashboard/table/card redesign. No automatic merge or deployment. See docs/authentication.md.

## Previous responsive verification (historical)

# MerchantOS Admin — mobile iteration 3

Updated 31 October 2026 (Asia/Karachi).

Branch: `feature/stitch-mobile`, based on merged tablet main `7fadb7df5266a58fd0fd2624cca94d1e6d372e6a`. Desktop PR #2 and tablet PR #3 are merged. [Mobile PR #4](https://github.com/danishirfan21/MerchantOS-Admin/pull/4) targets main and remains unmerged.

## Implemented

All five supplied Stitch mobile sources inspected and rendered at 390 × 844: Overview, Products, Inventory, Orders and Order Details. Shared compact header and four-link bottom navigation; actual-data KPI grid/chart; synchronized product and inventory cards; order cards; full-width product drawer; stacked receipt with a back header and one print action. Native search/filter/sort forms preserve existing backend behavior and no-JavaScript fallbacks. No mobile bottom bar on receipts.

Mobile screen CSS is isolated below 768px. Shared business calculations, read-only API access, UTC semantics, seven-row pagination, latest-100-order window, five-unit demo stock rule and print styles remain unchanged. The three template checks now verify both product table and card templates. A four-glyph local icon subset and the supplied five mobile product illustrations are used only by the mobile presentation.

## Executed local verification

- Release build: passed, zero warnings/errors.
- .NET tests: 33 passed, zero failed/skipped.
- Template checks: 3 passed, zero failed, both card/table templates checked.
- Mobile presentation checks: 14 passed, zero failed, using the explicit local contract fixture. These do not verify the real API/database. The additional real-order search/date/payment/pagination test runs in CI against ten persisted receipts.
- Desktop and tablet: all ten viewport screenshots pixel-identical to pristine merged main under identical local fixture data. All 15 desktop and 18 locally applicable tablet checks passed; the remaining tablet order-pagination test requires live CI.

## Real integration and complete responsive verification

Passing implementation snapshot: `4a690b1424d60d10dff0ec8d8d690a5422c9e956`.

- [Passing GitHub Actions run 41118720798](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41118720798).
- 33 .NET tests, 3 template checks, 15 desktop + 19 tablet + 15 mobile Chromium tests passed. Zero failures, skips or retries; Release builds zero warnings/errors.
- Actual pinned POS API + PostgreSQL 17 schema/seed/migrations and checkout/idempotency/concurrency checks passed. All 30 catalog/detail/inventory records, 10 persisted receipts and four primary routes verified.
- [Actual screenshots across all 15 screen variants, full-page tablet/mobile captures, mobile navigation/drawer captures, all three print PDFs, browser/TRX reports and logs](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41118720798/artifacts/13006868298).
- Additional mobile inventory refinement: compact attention-panel icon link retains its accessible filter name and 44px target. Its rendering, filters, 390/360px control fit and clearance checks passed locally.

The final refinement/documentation commit runs the complete suite again. Final-head CI/artifact links are recorded in PR #4. No desktop/tablet test file was modified.

## Visual review and scope

See [mobile visual review](docs/mobile-visual-review.md). No edits to POS, writes, second database, refund/payment-provider logic or public deployment. Illustrative Stitch customer/terminal/compliance/analytics data and unsupported add/export/settings/send-receipt controls are omitted. Desktop and tablet layouts are regression baselines. Historical tablet evidence follows.

---

# Tablet iteration 2 verification (historical)

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
