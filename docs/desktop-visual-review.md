# Desktop visual review — Stitch iteration 1

## Reference and method

The user supplied five complete Google Stitch HTML screens: Overview, Product Catalog, Inventory, Orders, and Order Details. They are the visual authority; their layouts were translated rather than replaced with a new design. Their Tailwind styling was expressed through Bootstrap components and maintainable custom CSS; Tailwind is not an application dependency.

Local Chromium rendered both the supplied HTML and the actual MVC application at 1440 × 900. The reference viewer served the supplied assets locally and used local fonts; the unavailable shared logo image was represented by the MerchantOS brand mark. Local MVC comparisons used the explicitly isolated POS contract fixture; final published application screenshots must come from real POS/PostgreSQL CI.

The local measurements on all five actual pages were: 256px sidebar, 64px header, 24px primary heading, local Inter font, and no document-level horizontal overflow. Screenshots were viewed for every page. A second correction pass reduced catalog summary cards to the reference's compact size, split filter/table surfaces, applied screen-specific margins, gave stock alerts shaded rows, aligned the receipt header, and tightened the financial summary. A print PDF was rendered and visually inspected.

## Screen checks

| Screen | Reference features preserved | Functional adaptation |
| --- | --- | --- |
| Overview | Four cards, 2:1 chart/alert columns, recent-orders table, shaded stock alerts | Real latest-100-order KPIs and nine hourly buckets on the latest completed date; no fabricated today sales |
| Products | Compact four-card strip, search/filter toolbar, 40px thumbnails, seven-row table, badges and pagination | Actual names, SKUs, prices, categories and stock; numeric sorting; encoded read-only detail drawer |
| Inventory | Four summaries, attention banner, quantity/threshold/status columns | Search/filter controls required by the brief; global demo threshold of 5; no stock writes |
| Orders | Heading/count badge, search/date/payment controls, table and pagination | Filters limited to the API's returned window; missing item counts shown as receipt links |
| Order details | White receipt header, purchased-item table, two-column financial/payment layout | Backend totals retained, line amounts displayed from persisted unit prices/quantities, cash change and working print output |

## Deliberate differences from static example data

- Static Stitch figures, fake products/SKUs, customer profiles and multi-store stock are not operational data.
- Settings, editing, price override, email receipt, refund and unimplemented account controls were removed rather than presented as functioning features.
- The sidebar identifies a read-only workspace rather than implying an authenticated administrator.
- The absent logo source is replaced by a simple related MerchantOS brand mark. Product photos are illustrative assets supplied in the reference, only mapped where the actual seeded product matches; remaining products use existing POS category illustrations.
- Date/window/threshold labels remain visible even where this changes a subtitle. UTC date filters cannot imply access to older orders beyond the latest 100.
- The financial summary omits the static amount-in-words sentence and unbacked compliance/payment claims. It prints the backend record rather than recalculating checkout logic.

These are intentional data and capability adaptations. This is a close visual translation, not a claim of pixel-identical output with different live data. New tablet/mobile designs remain deferred.
