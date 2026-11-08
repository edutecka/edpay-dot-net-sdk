# Roadmap & Plano de Arquitetura - EDPay .NET SDK

## Objectivos
1. Cliente assíncrono baseado em `HttpClient` e `System.Text.Json` com suporte a Dependency Injection (`services.AddEDPayClient(...)`).
2. Compatibilidade multiplataforma (.NET Standard 2.0, .NET 7.0, .NET 8.0).
3. Verificação de HMAC SHA-256 usando `CryptographicOperations.FixedTimeEquals`.

---

## Fases de Desenvolvimento

### Fase 1: Core Client & Autenticação (Concluído)
- [x] Headers `X-EDPay-Public-Key` e `X-EDPay-Private-Key`.
- [x] Suporte a idempotência via `Idempotency-Key`.
- [x] Alternância automática entre `Production` e `Sandbox`.
- [x] `WebhookVerifier` com `HMACSHA256` e `CryptographicOperations.FixedTimeEquals`.

### Fase 2: Módulos de Recursos (Concluído)
- [x] `PaymentsResource`: `CreateAsync`, `RetrieveAsync`, `CancelAsync`.
- [x] `CheckoutResource`: `CreateSessionAsync`, `RetrieveSessionAsync`, `ExpireSessionAsync`.

### Fase 3: ASP.NET Core Middleware & Extension Methods (Em Planeamento)
- [ ] `services.AddEDPay(options => ...)`
- [ ] Middleware para validação automática de assinatura de webhooks.

### Fase 4: NuGet & CI/CD
- [ ] Workflow GitHub Actions para build, testes unitários e empacotamento NuGet.
- [ ] Publicação no NuGet.org.
