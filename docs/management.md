# Product and inventory administration

## Trust boundary and contracts

The current user's backend session stays inside Admin's native server ticket. ManagementClient forwards it per request using PosSessionHandler (pooled handlers have cookies disabled), fetches an authenticated backend CSRF token, then posts a narrowly projected DTO. Browser POSTs independently require MVC antiforgery. Anonymous Management routes return401; expired sessions cannot write. All actions require Administrator; POS independently applies separate ProductManagement, InventoryManagement and AuditRead policies. Client-supplied actor IDs/roles are never forwarded.

Management was first verified at POS `12eff7f1f772e52d6ff13c1c00a1c6f2769afad4`; current reporting CI pins a later descendant in .github/workflows/verify.yml (see reporting.md); its 16 .NET, 21 Angular unit, 21 browser, 99 management and 39 security checks passed. POS PR #4 documents API contracts; that commit must be retained when merging POS before Admin. Security PR #5 is already merged.

| API | Contract |
|---|---|
| GET /api/admin/products/{id} | Current names/category/priceHalalas, SKU, product version, stock and stockVersion |
| POST /api/admin/products/{id} | name,nameAr,category,priceHalalas,version; strict unknown-field rejection |
| POST /api/admin/inventory/{id}/adjustments | operation,quantity,reason,stockVersion,idempotencyKey |
| GET /api/admin/audit | entityId/action/UTC from/to/page/pageSize; bounded results and hasMore |

Names 1–240 chars; six existing categories; price 0.01–1000000.00SAR converted with exact decimal arithmetic to integer halalas. No description/active flag exists, so no decorative schema is added. Product IDs/SKUs are immutable. Product optimistic version detects competing edits. Current catalog/new sales reflect price updates; persisted receipt snapshots do not change.

Stock operations: restock adds positive quantity; damage subtracts positive quantity; correction sets a nonnegative physical count. Server computes balances under inventory row locks shared with checkout. The stock version advances on every update through a PostgreSQL trigger, including sales and changes returning to the same stock count. Every adjustment requires the expected version; stale requests return409. A reason of1–500 chars is mandatory. Quantities are32-bit integers; server rejects negative/overflowed balances.

Adjustment keys are scoped to the authenticated actor. Identical retries replay the original result; changed payload reuse returns409. On an uncertain network/server failure the form freezes its values and retains the serialized payload/key for safe retry. Concurrent submission is guarded and controls disabled while pending. Product saves use optimistic conflict detection; uncertain product retries may conflict and require review rather than overwrite.

## User interaction

Open existing product details, choose Edit Product, Adjust Stock or View History. Forms prefill authoritative values. Changes submit only on explicit save; Cancel returns to details. Adjustment preview is informational, followed by a review summary with reason and Confirm Adjustment. Successful results display server previous/new stock, reason and UTC timestamp, then refresh affected rows/summary metrics while keeping query/pagination. Conflicts offer Refresh and review without automatic overwrite. Network, validation, permission and expired-session errors display text, not color alone.

Bootstrap's existing offcanvas retains focus trapping/Escape/trigger restoration. Form headings receive focus, controls have labels and mobile44px actions, errors are live alerts and focused after failure. Razor/Handlebars/textContent preserve encoding and restrictive CSP. History is 5 records/page, readable stacked before/after fields on mobile, with previous/next navigation. History has no mutation API.

## Persistence and setup

Apply POS migration 005_management.sql after003/004, with ON_ERROR_STOP, to the existing database. It adds versions, admin_audit and inventory_adjustments, indexes, stock-version and history-immutability triggers without altering balances/receipts/users/sessions. Every business write and audit insertion share a transaction; audit failure rolls back the write. Ordinary UPDATE/DELETE/TRUNCATE on either history table is blocked. Database owners can alter schema, so operational DB access still requires separate controls.

Use README and docs/authentication.md for local HTTPS/environment-based bootstrap. No new infrastructure or database is needed. Test only in isolated CI databases: real API tests include forced audit failure, migration replay and adjustment/checkout races. Existing61 browser and39.NET/3template checks are retained; added management cases exercise all three viewports. Reports omit traces and browser state and pass a credential audit.

## Limits

No bulk changes, creation/deletion, suppliers, refunds or user management. Audit is scoped to administrative product/inventory changes, not complete security-event logging or historical checkout inventory ledger. Pagination is bounded offset pagination; concurrent additions can shift pages, so refresh for current history. Production hardening remains required; no public deployment.
