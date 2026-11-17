# Changelog

## 1.6.2

- Align SDK documentation with the API's private-checkout wait: the provider's 60-second PIN window starts after Master dispatch; the API deadline is 80 seconds from the request, and the SDK transport default is 90 seconds.

## 1.6.1

- Remove the SDK environment selector; matching API key prefixes determine test or live access.
- Keep API requests on the single EDPay API host and update NuGet metadata to 1.6.1.
- Wait up to 80 seconds from the API request for M-Pesa/e-Mola private checkout. The provider's separate 60-second PIN window starts after Master dispatch, and dispatch latency consumes the API deadline; SDK transport defaults to 90 seconds.

## 1.6.0

- Add the unified payment parent and child checkout operations for private API, hosted checkout, and link checkout.
- Add opaque-token buyer routes and checkout history on the parent payment.

## 1.5.0

- Improve payment method handling and idempotent payment requests.

## 1.4.0

- Added customers, link checkout under the payment resource, wallets, FX quotes, webhooks, and public API resources.
- Completed payment, checkout, invoice, invoice-series, customer, and webhook methods against mounted API routes.
- Added validation for list limits and cursor pagination for wallet ledger entries.
- Added method-aware contact presets: eMola validates nine national digits with `86`/`87` and sends national digits; M-Pesa validates `84`/`85` and sends `258` plus national digits. Non-phone and custom gateway values pass through unchanged.
- Added explicit `limit` overloads for invoice and invoice-series lists, and normalized method-name separators in phone presets.
- Public payment-method and token-based checkout requests omit partner credentials while preserving idempotency keys on checkout payments.
- Added public support-chat calls, error request metadata, and timestamped webhook verification with a five-minute replay window.
