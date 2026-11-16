# Changelog

## 1.5.1

- Require a valid public/private API key pair from the same environment before creating the client.

## 1.5.0

- Add child payment-attempt methods for server payments and hosted checkout sessions/payment links. Attempts reuse the payment ID, amount, and reference.
- Keep method attempts idempotent and document when a failed, processing, or unknown result allows a new attempt.

## 1.4.0

- Added customers, payment links, wallets, FX quotes, webhooks, and public API resources.
- Completed payment, checkout, invoice, invoice-series, payment-link, customer, and webhook methods against mounted API routes.
- Added validation for list limits and cursor pagination for wallet ledger entries.
- Added method-aware contact presets: eMola validates nine national digits with `86`/`87` and sends national digits; M-Pesa validates `84`/`85` and sends `258` plus national digits. Non-phone and custom gateway values pass through unchanged.
- Added explicit `limit` overloads for invoice and invoice-series lists, and normalized method-name separators in phone presets.
- Public payment-method and token-based checkout requests omit partner credentials while preserving idempotency keys on checkout payments.
- Added public support-chat calls, error request metadata, and timestamped webhook verification with a five-minute replay window.
