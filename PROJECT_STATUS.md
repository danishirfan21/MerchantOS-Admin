# Milestone 6 — reporting implementation under review

Branch feature/sales-reporting-dashboard; [PR #7](https://github.com/danishirfan21/MerchantOS-Admin/pull/7), unmerged. Based on verified management main61489430eff4a3843579ee08dba121128b36e8c0. Depends on POS PR #5; exact backend revision is pinned in .github/workflows/verify.yml and docs/reporting.md.

Historical server-side Orders, store-local Overview periods, authoritative financial KPIs, accessible revenue trends, cash/simulated-card breakdown, snapshot top products and bounded CSV implemented. Current inventory/catalog remain explicitly labeled. Existing management/audit/checkout/authentication and receipt printing are preserved. No public deployment.

Local 56 .NET and 3 template checks passed. All 79 existing browser cases remain; 15 new reporting cases cover three viewports. Final CI and visual screenshots are recorded in PR #7 after execution. No pending result is described as passed.

Milestone 5 PR #6 merged as61489430eff4a3843579ee08dba121128b36e8c0; POS PR #4 merged asad73c8e406fc1b42c8fdf4fd88b721d08f2ce320. Reporting contracts, exact money/date definitions, query indexes, offset consistency, CSV bounds and known limitations: docs/reporting.md.

## Earlier implementation evidence

# MerchantOS Admin status

## Milestone 5 — implementation under review

Branch feature/product-inventory-management; [PR #6](https://github.com/danishirfan21/MerchantOS-Admin/pull/6), merged as 61489430eff4a3843579ee08dba121128b36e8c0. Based on verified security main `8e1ceb33835424f5fec342671ebfa74338c9a3e3`. Depends on [POS PR #4](https://github.com/danishirfan21/MerchantOS-POS/pull/4); exact backend ref is pinned in .github/workflows/verify.yml.

Product editing, reviewed inventory adjustments and bounded audit history implemented within the existing drawer. Typed authenticated server-side writes preserve both MVC and backend CSRF. Optimistic conflicts, safe retry, duplicate submission, query-preserving row/summary refresh and session failures are handled. Original dashboard/order/receipt layouts and financial calculations remain intact. See docs/management.md for contracts and limitations.

Local 56 .NET tests pass, preserving39 previous cases. Three template checks and original 61responsive browser cases remain, with18 new management browser cases (6 scenarios × 3 viewports). Initial CI41206295568 passed 47 .NET / 3 templates / 76 browser tests and real management/security integration. Final CI/screenshots and exact verified heads are recorded in PR #6 after execution; no pending result is claimed as passed here.

## Merged baseline

- Foundation/desktop/tablet/mobile PRs#1–4 merged, ending at64b278a180083998c6888a5d1effc83dd85cbd7c. Original33.NET/3template/49responsive cases passed.
- Authentication PR #5 merged as8e1ceb33835424f5fec342671ebfa74338c9a3e3. [Post-merge CI](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41204410070) passed39 Admin.NET,9 POS.NET,3 templates,61 browser, real PostgreSQL checkout and39 direct security checks.
- POS security PR #3 merged asab2b6397d36f3cd2c1597ccc50fbeb0c5a5d8ba5; both applications require real sessions.

Historical visual rationale remains in docs/*-visual-review.md. All15approved screen variants remain regression targets. Current new controls are confined to product details and management forms; no Stitch redesign.

## Limits

Local/isolated demo only; no public deployment. No second database, creation/deletion, refunds or expanded permissions system. Single-instance ticket cache, production keys/provisioning, MFA/recovery and distributed infrastructure remain future work. Historical reporting now replaces the earlier latest-100-order limit; stock threshold 5 is a presentation rule. See README and authentication/management docs.
