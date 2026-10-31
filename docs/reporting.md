# Historical sales reporting

## API and trust boundary

Admin uses its authenticated typed PosClient to GET /api/admin/reports/orders, /sales and /orders.csv. The current user's backend cookie stays server-side through PosSessionHandler; no new credentials, browser tokens, database or identity model. POS enforces an explicit HistoricalReporting Administrator policy, independent of MVC. Cashiers retain only existing catalog/checkout/owned-receipt access. Endpoints are read-only, no-store and return401/403 appropriately.

CI pins POS `f1b325578383b2c5c3e06c60e17093113223c311` in .github/workflows/verify.yml. Review POS PR #5 first and retain its tested commit when merging. Final dependency heads and CI are recorded in Admin PR #7; both current PRs stay unmerged. Prior Milestone 5 PRs#4(POS)/#6(Admin) are merged.

## Store time and filters

Store timezone is fixed Asia/Riyadh (UTC+03), not host/browser timezone. UTC timestamps remain persisted. Today is the current store calendar day; Yesterday is its preceding day; Last7Days/Last30Days include today and the preceding6/29days. Custom ISO from/to are inclusive selected store dates and convert to UTC [start midnight,end+1 midnight). One-day Jan2 starts Jan1 at21:00UTC. Custom ranges are1–366days, from year2000, with no future selected days. Reversed/malformed/future/overlong dates reject. Empty past ranges return legitimate zeroes, not errors.

Overview defaults Today; Orders defaults All history and additionally supports order-number substring search, cash/card and completed/cancelled/failed status. Failed payment attempts ordinarily create no order and thus no reporting revenue. Changing filters resets the page; links and exports retain filters. Query validation errors render explanatory text/Unavailable with HTTP400; unreachable reports show Unavailable rather than zero. Native navigation retains browser back/forward and GET functionality; controls announce Loading on submission. Session expiration redirects to login.

## Financial definitions

Only completed SAR orders contribute to sales metrics. Gross=sum persisted subtotal before discount/VAT. Discounts=sum discount. Net before VAT=gross-discounts. VAT=sum persisted VAT. Sales revenue=sum final payable including VAT. Average order=revenue/completed count rounded half-up to integer halalas; empty average0. No floating-point currency arithmetic. No order-level aggregation joins to payment/item rows, avoiding fan-out double counting.

Payment breakdown uses persisted cash/simulated-card order methods and counts, tax-inclusive revenue and decimal revenue share. These are demo sales, not externally settled provider records. Top products aggregate historical quantity×unit price before discounts/VAT; latest historical sale name per product identifies it. Current edited product prices never reconstruct historical money.

Single-day reports show24store-hour buckets,2–31days show daily buckets and longer custom periods show month buckets clipped to the range. Missing buckets are zero-filled; SVG bars show actual tax-inclusive revenue, with no smoothing/interpolation. An accessible expandable table exposes every bucket's value/count. No-sales periods show a clear empty state. Catalog/inventory/stock alerts are current state, independent of historical sales period.

## Pagination and export

SQL uses created_at DESC,id DESC with matching count and rows in one repeatable-read, read-only transaction. Admin requests7rows/page to preserve layout (API defaults20/max100), renders a bounded page-link window, and never fetches all history. Offset pagination can shift between requests when new orders arrive; deep offsets scan more data. It is not a cross-request snapshot. Date/filter count means matching orders, not completed-sales count. The recent-orders panel requests only5rows for the selected period.

CSV export shares backend WHERE/date semantics, ignores pagination and includes UTC plus store-local timestamp, order number, method/status, subtotal, discount, VAT, final total and currency. All text cells are quoted, quotes doubled, newlines preserved and formula-leading cells prefixed with apostrophe. UTF-8 BOM and safe fixed filename; maximum10,000rows,413if exceeded before output. Admin displays an actionable export-error page. Exports are bounded in memory, not unlimited database dumps. Receipt PDF printing remains unchanged.

## Database and verification

Apply POS migration006_reporting_indexes.sql after005, using ON_ERROR_STOP during a local maintenance window. It only adds chronological, completed-SAR covering and order-item lookup indexes. Never reset populated data. Existing authentication setup/provisioning remains in docs/authentication.md; apply all versioned POS migrations and run both applications locally with HTTPS or their explicit loopback-only exception.

POS reporting tests build10,000synthetic historical multi-item orders in isolated CI, plus exact UTC boundary, failed status and other-currency controls. Tests exercise100pages, arithmetic, snapshots, CSV formula/quote/newline behavior, export cap, readonly persistence, migration replay, role/session rejection and EXPLAIN ANALYZE/BUFFERS. The first verified single-day aggregation measured0.058seconds; this is a controlled fixture observation, not a production SLA. Query plans, screenshots, CSV and receipt PDFs are credential-audited before upload; traces remain off. Browser retries remain0.

Existing 52 .NET / 3 template / 79 browser coverage is preserved. Old UTC filter labels now assert Asia/Riyadh; the old latest100scope assertion now checks complete history. The unit matching-count expectation now checks the filtered server total. No browser case/assertion was deleted. New reporting scenarios verify presets, exact financials, custom/empty/invalid states, >100pagination, filter retention, receipt navigation, CSV and sessions across all three viewports.

## Limits

No refunds, external settlement reconciliation, multi-currency conversion, multi-store, forecasting, warehouse, external analytics or public deployment. Indexes do not make arbitrary substring search constant time; reads scale with selected data and have10-second query timeout. Monthly buckets report only the selected partial month. Separate page and analytics requests can observe later committed sales between requests; each response is internally consistent. Existing local-demo production security limitations remain.
