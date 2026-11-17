# Roadmap & Architecture Plan - EDPay .NET SDK

## Objectives
1. Asynchronous client based on `HttpClient` and `System.Text.Json` with Dependency Injection support (`services.AddEDPayClient(...)`).
2. Cross-platform compatibility (.NET Standard 2.0, .NET 7.0, .NET 8.0).
3. HMAC SHA-256 verification using `CryptographicOperations.FixedTimeEquals`.

---

## Development Phases

### Phase 1: Core Client & Authentication (Completed)
- [x] Headers `X-EDPay-Public-Key` and `X-EDPay-Private-Key`.
- [x] Idempotency support via `Idempotency-Key`.
- [x] Automatic switching between `Production` and `Sandbox`.
- [x] `WebhookVerifier` for timestamped `Payment-Signature` HMAC-SHA256 headers, constant-time comparison, and a configurable replay window.

### Phase 2: API Resource Coverage (Completed in 1.4.0)
- [x] Payment records with private, hosted, and link checkout operations; customers, invoices, and invoice series.
- [x] FX quotes, wallets and cursor-paginated ledger entries, public payment methods, and webhooks.
- [x] Method coverage follows mounted routes; the API has no payment-cancel, session-expire, or invoice-series-detail route.
- [x] Method-aware contacts, public endpoint calls, API error metadata, and timestamped webhook verification.

### Phase 3: ASP.NET Core Middleware & Extension Methods (Planned)
- [ ] `services.AddEDPay(options => ...)`
- [ ] Middleware for automatic webhook signature validation.

### Phase 4: NuGet & CI/CD
- [ ] GitHub Actions workflow for building, running unit tests, and packaging for NuGet.
- [ ] Publication on NuGet.org.
