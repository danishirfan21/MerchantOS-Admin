# Tablet visual review — 768 × 1024

All five supplied Google Stitch HTML/CSS sources were inspected before changes. Attachment order was Overview, Inventory, Products, Orders and Order Details. The references share a 64px compact header, 24px body margins (32px on Products), 24px headings, Inter typography, white rounded panels, slate drawer and emerald accents. Existing local assets and all backend presentation contracts were reused.

## Layout translation

| Screen | Tablet presentation |
| --- | --- |
| Overview | Two-column KPI grid, full-width chart, recent orders, then low-stock alerts |
| Products | Three real-data summaries; full-width search; category/availability/sort controls; Product, Price, Stock, Status columns |
| Inventory | Four stock cards in two columns; compact attention banner; Product, Category, Available, Status columns |
| Orders | Search/UTC date/payment controls; Order Number, Date, Total, Status; payment secondary to total |
| Receipt | Back link, order plate, purchased items, financial summary, payment, one final print button |

The inventory reference's separate action column is replaced by the linked product name, matching the shared detail interaction without a cramped extra column. All seven desktop catalog columns and order columns remain at desktop width. On tablet the persisted unit price and quantity appear beneath each receipt item. The existing desktop print button is positioned at the end through CSS grid; no duplicate print action is created.

## Review method and corrections

Rendered the supplied reference HTML and running MVC app in Chromium at 768 × 1024, capturing all five pages and full-height receipts/dashboard for lower sections. Initial local visual review used the explicit seed contract fixture for layout only, never as real integration evidence. Checked margins, header, card proportions, table readability, section order and document/table overflow. Corrected the drawer backdrop stacking, touch target for Reset filters, catalog three-card arrangement, and receipt unit-price visibility. No new runtime Tailwind, fonts, CDN or images are required by MVC.

The raw reference font/image URLs are fetched only into the scratch comparison render where available; availability does not affect the application. Actual content differs from illustrative Stitch data, so exact pixel parity is not claimed. Catalog valuation, per-SKU thresholds, store/terminal identities, customer/compliance/payment-ledger text, notification/settings/account controls and invented item counts are intentionally omitted. The real five-unit demo threshold, latest-100-order scope and backend monetary values take precedence.

## Automated evidence

CI executes the unchanged 33 .NET tests and three template checks, live POS/PostgreSQL initialization, migrations and checkout/catalog integration, all 15 desktop browser tests and the same 15 at tablet size, plus four tablet-specific tests. Zero retries. Both screenshot sets, print PDFs, reports and logs are uploaded. The final run and actual results are recorded in PROJECT_STATUS.md after CI completes.
