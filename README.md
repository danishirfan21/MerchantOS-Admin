# Authentication integration

See [authentication architecture, HTTPS setup and provisioning](docs/authentication.md). Admin uses a server-managed HttpOnly session and the POS PostgreSQL identity authority. Provision accounts explicitly; no default credentials exist. The CI workflow pins the matching POS authentication revision.

# MerchantOS Admin

A server-rendered retail operations workspace built with ASP.NET Core MVC 10 and Razor. MerchantOS Admin complements [MerchantOS POS](https://github.com/danishirfan21/MerchantOS-POS) with read-only catalog, stock and order oversight. It does not implement checkout or maintain a second inventory database.

> Local demo only. Authentication is required by both applications. Neither application is approved for public deployment. Milestone 1 is verified against the real POS API and PostgreSQL 17; see [PROJECT_STATUS.md](PROJECT_STATUS.md).

## Desktop UI — Stitch iteration 1

The five approved Google Stitch desktop screens are translated into reusable Razor Views, Bootstrap components and custom CSS. The acceptance viewport is **1440 × 900**. Desktop remains the regression baseline. The tablet iteration below adds the approved 768 × 1024 layouts; the mobile iteration below completes the approved responsive layouts.

- Dark slate 256px sidebar, fixed 64px header, local Inter typography and Material Symbols, restrained emerald/neutral surfaces.
- Overview: four real KPI cards, a completed-order hourly chart, recent receipts and stock alerts.
- Products: summary cards, category/stock/search filters, numeric sorting, seven-row pagination and a read-only Bootstrap product drawer.
- Inventory: stock summaries, an attention banner, filters, seven-row pagination and the same read-only detail interaction.
- Orders: order-number, UTC date and cash/simulated-card filters with pagination inside the latest-100-order API window.
- Order details: persisted purchased items, halala-based line display, backend subtotal/discount/VAT/total values and a functioning print-friendly receipt.

See [visual review and deliberate adaptations](docs/desktop-visual-review.md) and [asset provenance](docs/design-assets.md).

## Tablet UI — Stitch iteration 2

At **768 × 1024**, the shared shell uses a compact 64px branded header and a 320px dark-slate Bootstrap offcanvas drawer. Navigation supports touch, keyboard, Escape, focus trapping and restored trigger focus. The expanded 256px desktop sidebar remains at widths of 1200px and above.

The five shared Razor pages adapt with two-column overview/inventory cards, three catalog summaries, four-column Products and Orders tables, and a vertically stacked receipt with one Print Receipt action after Payment Method. Product names open the same read-only details; all existing searches, filters, sorting and pagination remain available. No controller, monetary calculation, API contract or POS change is required.

See [tablet visual review](docs/tablet-visual-review.md). CI runs all 15 shared browser tests at each viewport plus four tablet-specific tests, with zero retries. Reports include both sets of screenshots and receipt PDFs.

## Mobile UI — Stitch iteration 3

The approved **390 × 844** experience completes the five pages with a compact header, four-destination bottom navigation, two-column KPIs, product/inventory/order cards and a vertical receipt. The receipt uses a back header and one final Print Receipt action. Bottom navigation respects safe-area insets and reserves content clearance; a 360px overflow check is included.

Mobile cards share the same server data, paging state and AJAX response as desktop/tablet tables. Category and stock filters remain visible; More filters & sorting reveals numeric sorting and fallback submit/reset controls. Without JavaScript all GET form controls remain exposed, and bottom links and product pages work normally. The existing product drawer fills mobile width with keyboard/touch dismissal and restored focus.

Mobile CSS loads only for screen widths below 768px, preserving print, desktop and tablet styling. No frontend framework, backend write endpoint, API request duplication or additional database is introduced. CI runs **15 desktop, 19 tablet and 15 mobile tests**, all 33 .NET tests, the three escaping/runtime checks across both product templates, and the real POS/PostgreSQL integration fixture. See [mobile visual review](docs/mobile-visual-review.md) and [project status](PROJECT_STATUS.md) for executed evidence.

## Pages

| Page | Behavior |
| --- | --- |
| `/` | Catalog counts, units on hand, stock alerts, recent completed sales |
| `/Admin/Products` | Search English/Arabic product names and SKU; category and stock filters |
| `/Admin/Inventory` | Stock summaries and filtered read-only inventory table |
| `/Admin/Product/{id}` | Current product, price and stock detail |
| `/Admin/Orders` | Latest 100 orders; search by order number within that window |
| `/Admin/Order/{id}` | Persisted receipt items, payment method, discount, VAT and totals |

## Architecture

`MVC controller → AdminService → typed IPosClient/HttpClient → existing POS API → existing PostgreSQL`

- C#/.NET 10, MVC controllers, Razor layouts, strongly typed DTOs and ViewModels.
- Bootstrap 5 supplies forms, tables and a responsive offcanvas navigation drawer.
- jQuery enhances catalog filters with debounced AJAX and stale-request cancellation.
- Precompiled Handlebars templates update paginated rows and navigation with escaped values. The runtime works under `script-src 'self'` without `unsafe-eval`.
- Razor GET forms remain functional without JavaScript. Details use ordinary server-rendered pages.
- All vendor assets are copied locally from locked npm dependencies. No CDN is needed.
- Typed asynchronous `HttpClient` has an eight-second timeout, cancellation propagation, safe error messages and detail 404 handling.
- Application code only sends GET requests to the POS. No database library, connection string, checkout implementation or write endpoint is included.

## Upstream contracts and boundaries

Inspected source is pinned to POS commit `68cc809edb2146e13632ff9cd316504b2dc5d25d`.

| Contract | Use |
| --- | --- |
| `GET /api/products` | Entire catalog and its authoritative `stock` snapshot |
| `GET /api/products/{id}` | Product detail |
| `GET /api/orders` | Latest 100 persisted orders, descending by creation time |
| `GET /api/orders/{guid}` | Persisted receipt and line items |

The existing schema relates `products` to `inventory_levels` by product ID, `orders` to `order_items` and `payments` by order ID, and idempotency keys to orders. Admin never accesses these tables directly.

Prices are integer halalas; Admin formats them by dividing by 100 using decimal arithmetic. Receipt totals are displayed exactly as supplied. The sales tile sums completed SAR orders in the fetched window; it is **not lifetime revenue or a daily/date-range report**. Counts of completed orders also cover that window. Revenue includes persisted discounts and VAT. Low stock is an Admin display threshold of 1–5 units, out of stock is zero; neither changes POS business rules. Inventory summaries cover the full catalog, independently of table filters.

Gaps: no pagination beyond the latest 100 orders, no historical inventory snapshot, no date-range reporting contract, no authorization and no admin write contracts. Future milestones must address these at the backend boundary.

## Development setup

Requirements: .NET 10 SDK, Node.js 24/npm, Git, Docker Compose for the existing POS stack. Keep the repositories as siblings.

```sh
git clone https://github.com/danishirfan21/MerchantOS-POS.git ../MerchantOS-POS
git -C ../MerchantOS-POS checkout 68cc809edb2146e13632ff9cd316504b2dc5d25d
# Start only the database, migrator and API; Angular is optional.
docker compose -f ../MerchantOS-POS/docker-compose.yml up -d --build
npm ci
npm run assets
dotnet run --project src/MerchantOS.Admin/MerchantOS.Admin.csproj
```

Open `http://127.0.0.1:5090`. POS API uses `127.0.0.1:5080`, PostgreSQL uses `127.0.0.1:5433`; optional Angular POS remains on port 4200. Compose applies the upstream schema/seed/migration. Run its initial schema only on a fresh local database; never reset a populated store.

### Browsing and reporting scope

Catalog/inventory filters and numeric sorting run on the full returned product list before seven-row pagination. Summary cards cover the entire catalog, independently of table filters. Order search, date filters (explicitly UTC), and payment filters run before pagination over only the latest 100 orders. The order-list contract omits item counts; the UI links to the receipt instead of inventing a count or issuing an N+1 set of receipt requests.

The dashboard's chart shows nine consecutive UTC hourly buckets on the latest completed order date; its caption makes that date explicit. It is not a fabricated today-only report. Sales KPIs still cover all completed SAR orders in the fetched window, independently of pagination. Low/depleted stock uses a documented **global presentation threshold of 5 units**, not backend-configured product thresholds.

### Configuration

`PosApi:BaseUrl` defaults to `http://127.0.0.1:5080/`. Override it with `PosApi__BaseUrl` or environment-specific .NET configuration; it must resolve to a loopback URI for this milestone. Keep a trailing slash. `ASPNETCORE_URLS` can override the Admin listen address; use a loopback address such as `http://127.0.0.1:5090`.

Do not commit real credentials. Admin needs no database credentials. CI's database password is an isolated, disposable test fixture value.

## Verification

```sh
npm ci
npm run assets
npm run test:assets
dotnet restore tests/MerchantOS.Admin.Tests/MerchantOS.Admin.Tests.csproj
dotnet build tests/MerchantOS.Admin.Tests/MerchantOS.Admin.Tests.csproj -c Release --no-restore
dotnet test tests/MerchantOS.Admin.Tests/MerchantOS.Admin.Tests.csproj -c Release --no-build
npx playwright install chromium
# With POS and Admin running, and at least one completed POS sale:
python3 scripts/verify-live.py
npm run test:e2e -- --project=desktop
```

The live verification script is read-only. On a fresh isolated CI database, the pinned POS checkout verification creates transactional sales before Admin's integration checks. Do not run the upstream checkout verifier on a valuable development database: it deliberately changes stock and creates test orders.

CI in `.github/workflows/verify.yml` restores/builds/tests .NET, checks safe templates, provisions PostgreSQL 17, checks out the pinned POS fixture, verifies its live endpoints and checkout, starts MVC, compares real API and Admin catalog/inventory/receipt results, and runs 15 desktop Chromium checks at 1440 × 900 with zero retries. The browser artifact includes five page captures and a print-receipt PDF. Screenshots, HTML report, TRX report and service logs are uploaded even on failure. Pull-request workflows have read-only repository permissions.

For explicitly isolated **contract-only** browser testing when PostgreSQL is unavailable:

```sh
python3 tests/fixture-server.py ../MerchantOS-POS/db/init/002_seed.sql
# In other terminals, run Admin and npm run test:e2e.
```

This fixture is test-only and is never started by the application or live CI workflow. Its catalog comes from the pinned POS seed; its single receipt is synthetic. Passing against it does not verify the actual POS backend or PostgreSQL.

## Desktop screenshots

Actual Razor page captures from the [passing desktop CI run](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41084505556), backed by the pinned POS API and PostgreSQL 17. [Download all screenshots, the receipt PDF and test reports](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41084505556/artifacts/12991545523). Exact results: **33 .NET tests, 3 template checks and 15 desktop browser tests pass**. See [PROJECT_STATUS.md](PROJECT_STATUS.md).

| Overview | Products | Inventory | Orders | Order details |
| --- | --- | --- | --- | --- |
| [View](docs/screenshots/desktop/overview.png) | [View](docs/screenshots/desktop/products.png) | [View](docs/screenshots/desktop/inventory.png) | [View](docs/screenshots/desktop/orders.png) | [View](docs/screenshots/desktop/order-details.png) |

## Security limitations

No authentication or authorization is implemented in this read-only milestone. A demonstration login would not secure the upstream API, so none is provided. Do not use real customer/payment data, expose these services publicly, or add writes until both applications have an appropriate authorization design. Card payments in POS remain simulated.

MVC output encoding, bounded filter input validation, safe Handlebars escaping, static asset delivery, a restrictive CSP, frame protection, no-store responses and automatic anti-forgery validation for unsafe MVC methods are enabled. No unsafe MVC methods are currently offered. These measures are not a replacement for authentication.

## Roadmap

1. **Milestone 1:** read-only MVC foundation verified; [PR #1](https://github.com/danishirfan21/MerchantOS-Admin/pull/1) remains open for review.
2. **Milestone 2:** authenticated and authorized product/inventory contracts, validation and audit trails, coordinated explicitly with POS.
3. **Milestone 3:** persisted operational reporting with honest pagination/date-range contracts.
4. **Milestone 4:** accessibility, UX polish, screenshots and portfolio finalization.
