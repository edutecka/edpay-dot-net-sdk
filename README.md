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

Every partner API request requires both `EDPAY_PUBLIC_KEY` (`pk_test_` or `pk_live_`) and `EDPAY_PRIVATE_KEY` (`sk_test_` or `sk_live_`) from the same environment. The SDK rejects missing, blank, or mismatched key pairs when creating the client. Public catalog requests and buyer-facing checkout requests authenticated by a checkout token do not use the partner key pair; never put the private key in browser code.

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

## Continue the same payment with another method

The payment ID and reference identify one payment. Present active gateways as choices; each selected method is a child attempt, with only one attempt in progress at a time. Do not create another payment when the buyer changes method. Create the next attempt only after the current one is definitively failed. If it is still processing or its result is unknown, retrieve it later and wait for reconciliation to avoid charging twice. Use a new idempotency key for each child attempt.

```csharp
// paymentId is the ID returned by Payments.CreateAsync; save it with your order.
// First check GET /v1/payments/{id}; call only when its status is "failed".
var attemptJson = "{\\"method\\":\\"<METHOD_CODE>\\",\\"phone\\":\\"+25884xxxxxxx\\"}";
var nextAttempt = await client.Payments.CreateAttemptAsync(
    paymentId, attemptJson, "ORDER-10492-ATTEMPT-2");
```

For a payment started through a hosted session or payment link, use `Checkout.CreateSessionPaymentAttemptAsync` or `Checkout.CreatePaymentLinkPaymentAttemptAsync` with that same payment ID and its session token/link ID. The current public checkout form accepts phone-based methods; use the partner payment resource for tokenized instruments.

## Function reference

Each row tells you what the call does and when to use it. Partner calls use the API key pair; public catalog and buyer checkout calls do not. Supply a stable `Idempotency-Key` for writes where required.

| Resource | Function | Endpoint | What it does / when to use it |
|---|---|---|---|
| Payments | `Payments.CreateAsync` | `POST /v1/payments` | Creates the parent payment and its first method attempt. Start a server-to-server payment; keep its ID and reference for later method attempts and status checks. |
| Payments | `Payments.CreateCheckoutAsync` | `POST /v1/payment-checkout` | Legacy alias for creating a payment; it does not create a hosted checkout session. Use only when migrating an integration that already calls this route. New code should use create. |
| Payments | `Payments.ListAsync` | `GET /v1/payments` | Lists payments visible to the current API credentials and environment. Reconcile or show a payment history. Use the limit argument where available. |
| Payments | `Payments.RetrieveAsync` | `GET /v1/payments/{id}` | Returns one payment and its current status. Check the result on your server after a redirect or webhook. |
| Payments | `Payments.EventsAsync` | `GET /v1/payments/{id}/events` | Returns the recorded status changes for a payment. Inspect the payment lifecycle or troubleshoot a status change. |
| Payments | `Payments.AttemptsAsync` | `GET /v1/payments/{id}/attempts` | Lists the payment's provider attempts; this method is read-only. See which method was tried and the outcome of each attempt. |
| Payments | `Payments.CreateAttemptAsync` | `POST /v1/payments/{id}/attempts` | Adds another dynamic method as a child attempt. The payment ID, amount and reference stay the same; this attempt uses its own Idempotency-Key. Use after the latest attempt definitively failed. If it is processing or unknown, retrieve the payment and wait for reconciliation to avoid a duplicate charge. A payment supports up to 20 attempts. |
| Hosted checkout | `Checkout.CreateSessionAsync` | `POST /v1/checkout/sessions` | Creates a short-lived checkout session from a payment link. Returns a token for the browser. Send a buyer to EDPay-hosted checkout or embed it on the exact HTTPS origin. |
| Hosted checkout | `Checkout.RetrieveSessionAsync` | `GET /v1/checkout/sessions/{token}` | Reads public session and checkout details. Pass the token returned by createSession. Load the hosted checkout page using its browser token. |
| Hosted checkout | `Checkout.GetPaymentLinkAsync` | `GET /v1/checkout/payment-links/{id}` | Reads the public details for an active payment link. Render the buyer-facing link checkout before a payment is submitted. |
| Hosted checkout | `Checkout.FramePolicyAsync` | `GET /v1/checkout/sessions/{token}/frame-policy` | Reads whether the session can be embedded and its frame policy. Check embed eligibility before placing checkout in an iframe. |
| Hosted checkout | `Checkout.CreateSessionPaymentAsync` | `POST /v1/checkout/sessions/{token}/payments` | Submits a selected phone-based method and buyer data to a public session; tokenized methods are not accepted by the current hosted form. Requires an idempotency key. Start payment from a hosted checkout form. The response means accepted for processing, not paid. |
| Hosted checkout | `Checkout.CreateSessionPaymentAttemptAsync` | `POST /v1/checkout/sessions/{token}/payments/{payment_id}/attempts` | Adds another phone-based method to the payment already created from this session. It keeps the same payment ID and reference; hosted checkout currently does not accept tokenized instruments. Use the same session token and payment ID after the previous attempt definitively failed. Wait when the result is processing or unknown. |
| Hosted checkout | `Checkout.RetrieveSessionPaymentAsync` | `GET /v1/checkout/sessions/{token}/payments/{payment_id}` | Reads the status of a payment created from that session. Show a buyer the latest processing result; verify settlement server-side too. |
| Hosted checkout | `Checkout.CreatePaymentLinkPaymentAsync` | `POST /v1/checkout/payment-links/{id}/payments` | Submits a selected phone-based method and buyer data from a public link; tokenized methods are not accepted by the current hosted form. Requires an idempotency key. Start a buyer payment directly from a payment-link page. |
| Hosted checkout | `Checkout.CreatePaymentLinkPaymentAttemptAsync` | `POST /v1/checkout/payment-links/{id}/payments/{payment_id}/attempts` | Adds another phone-based method to the payment already created from this link. It keeps the same payment ID and reference; hosted checkout currently does not accept tokenized instruments. Reuse the same link ID and payment ID after the previous attempt definitively failed. Wait when the result is processing or unknown. |
| Hosted checkout | `Checkout.RetrievePaymentLinkPaymentAsync` | `GET /v1/checkout/payment-links/{id}/payments/{payment_id}` | Reads the status of a payment submitted through a public link. Refresh the buyer-facing result for a link payment. |
| Payment links | `PaymentLinks.CreateAsync` | `POST /v1/payment-links` | Creates a fixed-amount public link. Creating the link does not itself create a payment. Collect payment for a fixed amount through a shareable buyer URL. |
| Payment links | `PaymentLinks.ListAsync` | `GET /v1/payment-links` | Lists links owned by the authenticated account and environment. Show or reconcile the account's payment links. |
| Payment links | `PaymentLinks.RetrieveAsync` | `GET /v1/payment-links/{id}` | Reads one link and its status. Check expiry, amount, or whether the link is still active. |
| Payment links | `PaymentLinks.DeactivateAsync` | `POST /v1/payment-links/{id}/deactivate` | Stops an active link from starting future checkout payments. Close a link that should no longer accept buyer payments. |
| Payment links | `PaymentLinks.CreateCheckoutSessionAsync` | `POST /v1/checkout/sessions` | Convenience wrapper that creates a hosted session for the supplied link. Use it instead of calling checkout.createSession when your SDK provides this wrapper. |
| Customers | `Customers.CreateAsync` | `POST /v1/customers` | Creates a customer record for the authenticated account. Save customer details before associating them with payments or invoices. |
| Customers | `Customers.ListAsync` | `GET /v1/customers` | Lists customers in the current account and environment. Build a customer picker or reconcile customer records. |
| Customers | `Customers.RetrieveAsync` | `GET /v1/customers/{id}` | Reads one customer record. Load a customer's saved details by ID. |
| Customers | `Customers.UpdateAsync` | `PATCH /v1/customers/{id}` | Updates the supplied customer fields. Correct or refresh a customer's details; use a stable idempotency key where required. |
| Invoices | `Invoices.CreateAsync` | `POST /v1/invoices` | Creates a draft invoice with its customer and line items. Prepare an invoice before issuing it to the customer. |
| Invoices | `Invoices.ListAsync` | `GET /v1/invoices` | Lists invoices, optionally limited to a page size. Show or reconcile invoices for the account. |
| Invoices | `Invoices.RetrieveAsync` | `GET /v1/invoices/{id}` | Reads one invoice and its current state. Load invoice details or check its lifecycle. |
| Invoices | `Invoices.FinalizeAsync` | `POST /v1/invoices/{id}/finalize` | Issues the draft invoice and makes it available for payment. The invoice is complete and ready to send to the customer. |
| Invoices | `Invoices.VoidAsync` | `POST /v1/invoices/{id}/void` | Voids an invoice that should no longer be payable. Cancel an issued invoice according to its allowed state transitions. |
| Invoice series | `Subscriptions.CreateAsync` | `POST /v1/invoice-series` | Creates a schedule that issues invoices on the configured interval. Bill a customer repeatedly using a defined start date, interval, and due period. |
| Invoice series | `Subscriptions.ListAsync` | `GET /v1/invoice-series` | Lists invoice series; there is no series-detail endpoint in this API version. Review recurring schedules for the account. |
| Invoice series | `Subscriptions.PauseAsync` | `POST /v1/invoice-series/{id}/pause` | Pauses future invoice generation for a series. Temporarily stop recurring billing without deleting the schedule. |
| Invoice series | `Subscriptions.ResumeAsync` | `POST /v1/invoice-series/{id}/resume` | Resumes invoice generation for a paused series. Restart recurring billing after the pause condition has ended. |
| FX quotes | `Fx.QuoteAsync` | `GET /v1/fx/quote` | Returns an indicative conversion quote for an amount and currency pair. Display a conversion estimate before creating a payment. A quote is not a settlement guarantee. |
| Wallets | `Wallets.ListAsync` | `GET /v1/wallets` | Lists wallets available to the authenticated account and environment. Choose a wallet before reading its balance or entries. |
| Wallets | `Wallets.RetrieveAsync` | `GET /v1/wallets/{id}` | Reads one wallet's details. Load the wallet metadata for an account view. |
| Wallets | `Wallets.BalanceAsync` | `GET /v1/wallets/{id}/balance` | Reads the wallet's current balance. Show the available or pending balance for a wallet. |
| Wallets | `Wallets.EntriesAsync` | `GET /v1/wallets/{id}/entries` | Lists ledger entries with cursor pagination. Show a transaction statement; pass the returned cursor for the next page. |
| Webhooks | `Webhooks.ListEndpointsAsync` | `GET /v1/webhook-endpoints` | Lists configured webhook destinations. Review which event destinations are active. |
| Webhooks | `Webhooks.CreateEndpointAsync` | `POST /v1/webhook-endpoints` | Registers an HTTPS receiver and selected event types. The signing secret is shown only at creation. Connect your server to receive payment lifecycle events; store the returned secret securely. |
| Webhooks | `Webhooks.UpdateEndpointAsync` | `PATCH /v1/webhook-endpoints/{id}` | Updates a destination's URL or subscribed event types. Change a receiver without creating a second endpoint. |
| Webhooks | `Webhooks.DisableEndpointAsync` | `POST /v1/webhook-endpoints/{id}/disable` | Stops future deliveries to an endpoint while retaining its record. Temporarily stop a destination while investigating or migrating it. |
| Webhooks | `Webhooks.DeleteEndpointAsync` | `DELETE /v1/webhook-endpoints/{id}` | Removes a webhook endpoint. Permanently remove a destination that is no longer used. |
| Webhooks | `Webhooks.ListDeliveriesAsync` | `GET /v1/webhook-deliveries` | Lists delivery attempts and their summary states. Monitor failed, pending, or completed event deliveries. |
| Webhooks | `Webhooks.RetrieveDeliveryAsync` | `GET /v1/webhook-deliveries/{id}` | Reads one delivery's status and attempt metadata. Inspect one delivery before deciding whether to retry it. |
| Webhooks | `Webhooks.RetryDeliveryAsync` | `POST /v1/webhook-deliveries/{id}/retry` | Queues a retry for an eligible failed delivery. Retry after fixing the receiver; the endpoint must be active. |
| Public catalog and optional support chat | `Public.ListPaymentMethodsAsync` | `GET /v1/public/payment-methods` | Returns active payment methods and their public requirements without partner keys. Build a method selector and validate the selected method's required fields. |
| Public catalog and optional support chat | `Public.SupportChatAsync` | `POST /v1/public/support/chat` | Submits a support-chat request and returns the server-sent event stream as text. Use only when integrating the public support-chat feature; this is not a payment operation. |
| Contact helpers | `PhoneNumber.Normalize` | `Local helper` | Removes display punctuation and returns the phone value in the configured national or international form. Prepare a phone value before validating or sending it to a method that requires one. |
| Contact helpers | `PhoneNumber.Format` | `Local helper` | Formats a valid phone number for display; it does not change the API payload automatically. Show a readable phone number in a form or confirmation screen. |
| Contact helpers | `PhoneNumber.AddCountryPrefix` | `Local helper` | Adds the configured country calling code to a national number. An integration needs the international form, such as 258 plus national digits. |
| Contact helpers | `PhoneNumber.RemoveCountryPrefix` | `Local helper` | Removes the configured country calling code and returns national digits. A selected method expects the national number without a country code. |
| Contact helpers | `PhoneNumber.IsValid` | `Local helper` | Checks length, allowed characters, country code, and configured prefixes; returns true or false. Reject invalid input before calling the API. It does not replace server-side validation. |
| Contact helpers | `PhoneNumber.RulesForPaymentMethod` | `Local helper` | Returns known contact rules for the selected method, or null when no preset exists. Load rules before validating a phone; configure rules from the live catalog for other phone methods. |
| Contact helpers | `PhoneNumber.NormalizeForPaymentMethod` | `Local helper` | Normalizes the contact using the selected method's preset rules; passes the input through unchanged when there is no preset. Call this helper directly with the method code and contact. |
| Contact helpers | `PhoneNumber.FormatForPaymentMethod` | `Local helper` | Formats the contact using the selected method's preset rules. Display the buyer's number according to the selected method's country rules. |
| Contact helpers | `PhoneNumber.IsValidForPaymentMethod` | `Local helper` | Validates the contact directly against a selected method's preset rules. Check a number in one call when the selected method has a known preset. |
| Contact helpers | `PhoneNumber.RequiresPhoneForPaymentMethod` | `Local helper` | Reports whether the SDK has a phone requirement preset for the method; unknown methods return false. Decide whether to show phone input only when the method's requirements are known. Check the live catalog too. |
| Webhook signature verification | `WebhookVerifier.Verify` | `Local helper` | Checks the Payment-Signature HMAC and timestamp against the unmodified request body. Verify the signature before parsing or acting on an incoming webhook. Use the raw bytes and keep the secret server-side. |
| Client utilities | `client.SendRequestAsync / SendPublicRequestAsync` | `Raw API request` | Sends a request to an API path without a dedicated resource wrapper. SendRequestAsync sends partner-key headers; SendPublicRequestAsync omits them. Choose the method that matches the endpoint's authentication. Use only for a supported endpoint that has no SDK method; keep private keys on the server and apply the endpoint's scope and idempotency rules. |

Resource methods return JSON text asynchronously. `EDPayException` exposes HTTP status, response, request ID, and retry-after metadata.
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
