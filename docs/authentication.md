# Admin authentication integration

Admin is a server-rendered backend-for-frontend using the authentication-enabled POS API as the only identity authority. POS stores users, roles, lockout state and revocable 30-minute sessions in its existing PostgreSQL database. Admin has no identity database and no long-lived service-account secret.

The login form posts credentials with MVC antiforgery validation to Admin. A dedicated server-side HttpClient obtains POS antiforgery tokens, authenticates, verifies the authoritative Administrator role and keeps the resulting backend cookie in an in-memory ASP.NET `ITicketStore` ticket. The browser receives only the protected, HTTP-only Admin session reference. Backend credentials are not serialized into browser JavaScript or browser token storage. Cookie handlers on pooled HttpClients are disabled; a per-request delegating handler forwards the current user's server-held session explicitly, preventing cross-user CookieContainer leakage.

Every authenticated MVC request validates the backend session and role again through `/api/auth/me`. Revoked, expired, disabled and role-changed sessions are rejected. Existing routes require Administrator; ProductRows returns 401 to anonymous requests, while HTML pages redirect to the sign-in form. Cashier sign-in to Admin fails with a generic error and the temporary backend session is revoked. The API independently enforces reporting policy, so manually calling the backend does not bypass MVC controls.

Sessions expire absolutely after 30 minutes and never slide. Logout is an authenticated antiforgery-protected POST that revokes the backend session before removing Admin state. Backend revocation failure returns a retryable error rather than falsely claiming successful logout. Local process restart loses the Admin ticket cache and requires sign-in again; this single-instance cache is intentional for the current demo. Backend sessions remain bounded by their absolute expiry. Future multi-instance deployments need a protected distributed ticket store, explicit trusted proxy settings and managed encrypted Data Protection key persistence.

## Setup

1. Use the exact authentication-enabled POS revision pinned in `.github/workflows/verify.yml` and linked in the security PR dependency. Apply its `004_identity.sql` migration after existing checkout migrations; existing data must not be reset.
2. Provision Administrator and Cashier development users with POS's documented `--bootstrap-user` command and environment-only credentials. No accounts are provisioned automatically by the applications; no public registration or known passwords exist.
3. Run POS API and Admin over HTTPS by default. Set `PosApi__BaseUrl` to its loopback HTTPS endpoint and configure Admin's HTTPS URL/development certificate. API calls remain on the server; frontend pages never need an API password.
4. For explicit loopback-only development/CI, both processes may set `ASPNETCORE_ENVIRONMENT=Development` and `Security__AllowLoopbackHttp=true`. Every HTTP request still checks a loopback Host and remote IP; this exception refuses non-Development startup. No authentication bypass exists in CI. Remote/internal-container HTTP is rejected. The production default keeps cookies Secure; all session/antiforgery cookies are HttpOnly and SameSite=Strict.
5. Browse a protected page, sign in as Administrator, and return to its original path/query. External and scheme-relative return URLs are normalized to `/` and navigation uses LocalRedirect. Use Sign out from the sidebar/drawer.

## Verification and artifacts

The original 33 .NET tests are preserved; rendering-only tests install an isolated test authentication handler within the test project. Six new HTTP unit tests verify anonymous redirects/401, CSRF and unsafe returns. The original 49 responsive browser assertions remain; fixtures sign in using real temporary identities against the real POS database. Four new authentication browser cases run at desktop, tablet and mobile sizes, covering login/logout, original queries, cashier denial, open redirects and CSRF. The login page uses the existing local typography/emerald palette without modifying the approved business-screen structures; only Sign out and the current user indicator are added.

CI generates and masks random passwords, provisions four temporary users in its isolated PostgreSQL, and pins the POS auth commit. Backend security checks additionally verify forged credentials/roles, 401/403, cashier ownership, CSRF, revoked/expired/disabled sessions, account lockout and IP throttling. All existing checkout arithmetic, idempotency and concurrency checks continue with authenticated Cashier sessions.

Traces are disabled because traces can contain cookies/password bodies. Temporary storage state files are mode 0600 under `/tmp` and never uploaded. A fail-closed upload gate scans files and embedded HTML report archives for actual ephemeral passwords and protected Data Protection cookie/CSRF values; findings block artifact upload. Test password fields are populated without secret-bearing fill step titles; no screenshots are taken during credential entry. Reports include logged-in screens and empty login forms only. No production credentials or signing keys are committed.

## Limits

This remains a local demo and is not approved for public deployment. MFA, password recovery/change, production account administration, detailed security audit logs, production encrypted key storage and distributed throttling are future work. POS owns all authorization; new backend endpoints deny by default until explicitly classified. The Administrator role is not checkout permission. No business editing, inventory adjustment, real payment processing or refunds are introduced.

## Executed evidence

[Implementation CI 41138596485](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41138596485) passed 39 Admin .NET, 9 POS .NET, three template and all 61 browser checks, with real authenticated API/PostgreSQL requests and 38 direct security/log assertions. [Screenshots and print reports](https://github.com/danishirfan21/MerchantOS-Admin/actions/runs/41138596485/artifacts/13017426719) cover the fifteen protected variants plus responsive login forms. One preexisting mobile test now explicitly waits for authenticated navigation to load before asserting AJAX behavior; its filtering assertions are unchanged. Final-head verification is linked in PR #5, pinned to the final POS security revision.
