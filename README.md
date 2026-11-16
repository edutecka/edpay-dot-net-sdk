# EDPay .NET SDK

EDPay .NET client for server-to-server payments and hosted checkout.

## Requirements

- .NET SDK 8 or later
- EDPay API keys for your environment

## Install

Build a local NuGet package from the SDK repository:

```bash
git clone https://github.com/edutecka/edpay-dot-net-sdk.git
cd edpay-dot-net-sdk
dotnet pack EDPay.Sdk.csproj -o ./artifacts
```

From your application's directory, install the package from the generated feed:

```bash
dotnet add package EDPay.Sdk --source /path/to/edpay-dot-net-sdk/artifacts
```

## Create a payment

Keep both API keys in server-side configuration, such as environment variables or .NET user secrets. `Payments.CreateAsync` takes the API request body as JSON and returns the response as JSON text.

```csharp
using System;
using EDPay.Sdk;
using EDPay.Sdk.Contacts;

var publicKey = Environment.GetEnvironmentVariable("EDPAY_PUBLIC_KEY")
    ?? throw new InvalidOperationException("Set EDPAY_PUBLIC_KEY.");
var privateKey = Environment.GetEnvironmentVariable("EDPAY_PRIVATE_KEY")
    ?? throw new InvalidOperationException("Set EDPAY_PRIVATE_KEY.");

var client = new EDPayClient(new EDPayOptions
{
    PublicKey = publicKey,
    PrivateKey = privateKey,
    Environment = EDPayEnvironment.Sandbox,
    BaseUrl = "https://api-pay.edutecka.com"
});

var method = Environment.GetEnvironmentVariable("EDPAY_METHOD")
    ?? throw new InvalidOperationException("Set EDPAY_METHOD from GET /v1/public/payment-methods.");
var phoneMember = "";
if (PhoneNumber.RequiresPhoneForPaymentMethod(method))
{
    var phoneInput = Environment.GetEnvironmentVariable("CUSTOMER_PHONE")
        ?? throw new InvalidOperationException("Set CUSTOMER_PHONE for this method.");
    if (!PhoneNumber.IsValidForPaymentMethod(method, phoneInput))
        throw new InvalidOperationException("Phone is invalid for the selected method.");
    var phone = PhoneNumber.NormalizeForPaymentMethod(method, phoneInput);
    phoneMember = $",\"phone\":\"{phone}\"";
}
var payload = $$"""
    {"amount_minor":50000,"currency":"MZN","method":"{{method}}"{{phoneMember}},
     "reference":"ORDER-10492","description":"Example order"}
    """;

string responseJson = await client.Payments.CreateAsync(
    payload,
    idempotencyKey: "ORDER-10492-CREATE-1");
Console.WriteLine(responseJson);
```

`amount_minor` is a positive integer in minor currency units (`50000` means MZN 500.00); `currency` must be `MZN`, `USD`, or `EUR` and match the account base currency. Select a live `method` ID from the public catalog. Set `CUSTOMER_PHONE` to the buyer's input in your own environment; the method-aware helpers validate and normalize eMola (`86`/`87` → 9 national digits, sent nationally) and M-Pesa (`84`/`85` → `258` + 9 national digits). For card and digital-wallet methods, `RequiresPhoneForPaymentMethod` is false and tokens go in `channel`; non-phone/custom values are not parsed as phone numbers. The API `channel` object supports `type`, `provider`, `customer_phone`, `customer_email`, `payment_token`, `wallet_token`, `return_url`, and `holder_name` as applicable. Optional payment fields also include `customer_id`, `reference`, and `description`. API key prefixes select the environment; sandbox and production use the same API host. Partner API routes send key headers; the public catalog and token-based checkout omit them. Writes include an `Idempotency-Key` where the endpoint requires one.

## Hosted checkout and webhooks

Create a payment link and persist a `payment_link_id` → local order mapping before redirecting the customer. Then create a hosted checkout session with `client.PaymentLinks.CreateCheckoutSessionAsync(jsonPayload, idempotencyKey)`. The session payload contains `payment_link_id`; the response returns a short-lived token for browser checkout. On webhook receipt, use `data.payment_link_id` to recover the order mapping; the field is nullable for payments that did not come from a payment link. Checkout resource methods also read frame policy and create or retrieve session and payment-link payments. Confirm the final payment server-side.

Verify webhook signatures against the exact raw request body bytes before processing an event:

```csharp
using EDPay.Sdk.Webhooks;

byte[] rawRequestBody = await ReadRequestBodyBytesAsync();
string signatureHeader = GetRequestHeader("Payment-Signature");
bool isValid = WebhookVerifier.Verify(rawRequestBody, signatureHeader, webhookSecret);
if (!isValid)
{
    // Reject the callback; do not process its event.
}
```

The header format is `Payment-Signature: t=<unix-seconds>,v1=<hex>`. The verifier computes HMAC-SHA256 over the timestamp, a period, and the unmodified body bytes, compares in constant time, and rejects timestamps outside the default five-minute tolerance. Pass a custom `TimeSpan` tolerance when needed. The `string` overload UTF-8 encodes the payload; use `byte[]` to preserve the exact HTTP body. Keep the full webhook secret returned at endpoint creation in protected server-side configuration.

Webhook payment data includes a nullable link reference:

```json
{"id":"evt_0123456789abcdef","object":"event","type":"payment.succeeded","created_at":"2026-11-07T12:05:00Z","data":{"object":"payment","id":"pay_0123456789abcdef","reference":"ORDER-10492","payment_link_id":"plink_0123456789abcdef","trans_id":null,"method":"mpesa","status":"succeeded","amount_minor":50000,"currency":"MZN","platform_fee_minor":3000,"receiver_net_minor":47000,"created_at":"2026-11-07T12:04:00Z"}}
```

Deserialize this shape with `WebhookEvent` / `WebhookPaymentData`; `PaymentLinkId` is nullable. Persist the link-ID-to-order mapping before redirecting the customer and use this field to recover the order. Handle `null` for other payment paths.

## Available operations

- Payments: create (`/v1/payments` and `/v1/payment-checkout`), list (limit 1–100), retrieve, events and attempts.
- Checkout: create session; retrieve session/payment link; frame policy; create/retrieve session and payment-link payments.
- Customers: create, list (up to 100), retrieve and update. Payment links: create, list (up to 100), retrieve and deactivate.
- Invoices: create, list (default or explicit limit 1–100), retrieve, finalize and void. Invoice series: create, list (default or explicit limit 1–100), pause and resume; no series-detail route exists.
- FX quote; wallets (list, retrieve, balance, cursor-paginated entries, limit 1–100); public payment methods; webhooks (endpoint list/create/update/disable/delete and delivery list/retrieve/retry, limit 1–100).
- Public support chat: post optional `session_id` and `messages[]` (`role`, `content`) JSON and receive the server-sent event stream as text (default timeout: 60 seconds; API stream can run for up to 45 seconds).
- Contacts: generic `Normalize`, `Format`, `AddCountryPrefix`, `RemoveCountryPrefix`, `IsValid`; method-aware `RulesForPaymentMethod`, `NormalizeForPaymentMethod`, `FormatForPaymentMethod`, `IsValidForPaymentMethod`, and `RequiresPhoneForPaymentMethod`.
- Webhook signature verification: `WebhookVerifier.Verify`

`EDPayException` exposes the HTTP status and response body, plus `RequestId` (`X-Request-Id`) and `RetryAfter` (`Retry-After`, when present). Honor the retry delay on HTTP 429.

Resource methods return JSON text. Request schemas use `amount_minor`, `currency`, `method`, conditional `phone`, optional `customer_id`, `reference`, `description`, and `channel` for payments; invoice `items[]` have `description`, integer `quantity`, and `unit_amount_minor`; invoice series add `interval_unit`, `interval_count`, `starts_at`, `due_days`, and optional `ends_at`; payment links use `amount_minor`, `currency`, optional `expires_at`; customers use `name` and optional `email`; webhook endpoints use `url` and `events[]`; checkout sessions use `payment_link_id`. Partner API errors use `{error:{type,code,message,details},meta:{request_id}}`. Deserialize responses with `System.Text.Json`; `EDPayException` includes status, body, request ID, and retry-after metadata.

## API documentation

- [SDK installation and integration guide](https://docs-pay.edutecka.com/sdks/)
- [Payments API](https://docs-pay.edutecka.com/api/payments/)
- [Authentication and API keys](https://docs-pay.edutecka.com/guides/authentication/)
- [Webhooks](https://docs-pay.edutecka.com/api/webhooks/)

## License

MIT.

## Other official SDK and platform repositories

These links identify official project repositories; a repository listing does not imply a published package or production-ready integration. Review each project's own status and requirements before use.

### SDKs

- [JavaScript / TypeScript SDK](https://github.com/edutecka/edpay-javascript-sdk)
- [Python SDK](https://github.com/edutecka/edpay-python-sdk)
- [PHP SDK](https://github.com/edutecka/edpay-php-sdk)
- [Java SDK](https://github.com/edutecka/edpay-java-sdk)

### E-commerce and billing integrations

- [WooCommerce (WordPress)](https://github.com/edutecka/edpay-woocommerce-plugin)
- [Shopify](https://github.com/edutecka/edpay-shopify-app)
- [Magento / Adobe Commerce](https://github.com/edutecka/edpay-magento2-module)
- [PrestaShop](https://github.com/edutecka/edpay-prestashop-module)
- [OpenCart](https://github.com/edutecka/edpay-opencart-extension)
- [WHMCS](https://github.com/edutecka/edpay-whmcs-gateway)
