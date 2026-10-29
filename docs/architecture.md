# Integration decisions

POS source reviewed: `services/merchant-api/Program.cs`, `CheckoutEndpoints.cs`, `Contracts/CatalogDtos.cs`, `db/init/001_schema.sql`, `db/init/002_seed.sql`, `db/migrations/003_checkout.sql`, `README.md`, Compose and its verified workflow, pinned at `68cc809edb2146e13632ff9cd316504b2dc5d25d`.

The product list endpoint already projects `Inventory.QuantityOnHand` as `stock`, so inventory does not require N+1 inventory requests. The full catalog is fetched once per MVC request; search/category/stock selection are deterministic in-process filters over that response. This also keeps complete inventory summaries and category options while the table is filtered. A larger catalog should add a dedicated paginated admin reporting contract rather than silently fetching unbounded data.

Dashboard catalog and order requests run concurrently. Each has its own unavailable state; zero is reserved for successful empty data, with unavailable labels on failed metric sources. No fabricated trend series or date filters are presented.

POS order list has a hard `LIMIT 100`. Fetching order details cannot recover older order IDs, so the Admin does not imply all-time reporting. Order detail uses the persisted receipt contract, preserving historical names/prices and supplied totals. Current catalog prices never replace historical receipt prices.

MVC deliberately has no write routes, database package or database connection. Checkout, row locks, advisory idempotency locks, discounts, VAT and payment simulation remain entirely in POS. Authorization and a dedicated backend read/write contract must precede future administrative writes.

Testing separates isolated unit/Razor tests, explicit local contract-fixture browsers, and real pinned backend/database CI. The fixture is excluded from the actual application startup path and CI live path. The application never falls back to fixture data during an outage.
