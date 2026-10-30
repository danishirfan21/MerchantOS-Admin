# Mobile visual review — 390 × 844

Inspected all five supplied HTML/CSS sources before implementation. Attachment order: Products, Overview, Inventory, Order Details, Orders. Rendered the approved Stitch sources and the actual running MVC app in Chromium at 390 × 844; captured viewport and full-page screenshots, inspected header/navigation, cards, chart, typography, filters, imagery and receipt section order, and iterated on meaningful differences. Also checks all five routes at 360px for overflow, controls and bottom-navigation clearance.

## Shared implementation

Mobile-specific screen styles live in mobile.css, linked with `media="screen and (max-width: 767px)"`. Mobile card/navigation partials remain hidden at larger widths and in print. Existing table markup, larger breakpoints, API contracts, controllers, calculations and print rules are preserved. Product table and card templates use the same authoritative AJAX result; no extra catalog request or second filter/pager state. Server cards and native forms provide the no-JavaScript fallback.

The four primary destinations share a safe-area-aware bottom navigation and the existing Bootstrap drawer. Receipt details omit that bottom bar and use a compact back header. Product details reuse the encoded Razor partial and existing focus/scroll management, with a full-width mobile drawer. A local four-glyph Material Symbols font avoids missing-ligature text overflow without changing the desktop/tablet icon font.

| Screen | Implemented mobile structure |
| --- | --- |
| Overview | Four two-column KPIs, supported hourly chart, compact recent-order cards, low-stock rows |
| Products | Search, category/stock filters, optional sorting controls, thumbnail/name/SKU/status/price/quantity cards, paging |
| Inventory | Four two-column stock cards, compact attention panel, same filters, quantity-first inventory cards, paging |
| Orders | Search, UTC date/payment filters, order/date/payment/total/status cards, working receipt links and paging |
| Receipt | Back header, wrapping order ID/status/date, item cards, financial summary, payment, one Print Receipt action |

## Deliberate differences from illustrative Stitch data

- Uses real POS data and existing semantics: latest 100 orders, UTC dates, completed sales and global demo low-stock threshold of five units. No invented today-only sales, branch/terminal names, item counts absent from list responses, compliance, balance-due or bank reconciliation records.
- Keeps shared seven-row pagination. Sorting and progressive submit/reset controls sit behind a mobile disclosure; without JavaScript every control is visible. Native selects replace illustrative custom popovers.
- Add product, barcode scanning, unsupported export/settings and Send Receipt are omitted. Existing receipt printing and detail navigation work normally.
- The five supplied mobile photographs are matched to existing seeded identities (p01/p02/p06/p08/p13). Other products retain the existing category illustrations. They remain illustrative catalog images, not verified product photography.
- Source materials and actual API content differ, so no exact mobile pixel-parity claim is made.

## Verification

Local presentation review uses the explicit contract fixture only. Actual API/PostgreSQL evidence comes from CI: schema/seed/migrations, upstream checkout/idempotency/concurrency, 30 catalog/detail/inventory records and ten persisted receipts, followed by all 15 desktop, 19 tablet and 15 mobile browser checks, zero retries. Desktop/tablet screenshot comparisons use pristine merged main and identical data. Both baseline assertions are preserved without weakening. CI uploads all viewport/full-page mobile captures, navigation/drawer captures, reports/logs and all three receipt PDFs. Final execution results and links are recorded in PROJECT_STATUS.md and the PR.
