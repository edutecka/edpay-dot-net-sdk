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
});

var payload = """
    {"amount_minor":50000,"currency":"MZN",
     "reference":"ORDER-10492","description":"Example order"}
    """;

string responseJson = await client.Payments.CreateAsync(
    payload,
    idempotencyKey: "ORDER-10492-CREATE-1");
Console.WriteLine(responseJson);
```

`Payments.CreateAsync` creates only the parent payment. Choose a dynamic method afterward through `PrivateCheckoutAsync`, or let the buyer choose in hosted or link checkout. Method-specific fields such as phone and channel belong to that child operation. `amount_minor` is a positive integer in minor currency units (`50000` means MZN 500.00); `currency` must be `MZN`, `USD`, or `EUR` and match the account base currency. Optional payment fields also include `customer_id`, `reference`, and `description`. API key prefixes select the environment; sandbox and production use the same API host. Partner API routes send key headers; the public catalog and token-based checkout omit them. Writes include an `Idempotency-Key` where the endpoint requires one.

## Choose a checkout mode for one payment

`Payments.CreateAsync` creates the parent record only. Save its ID and reference with the order, then choose a child integration. The same payment can use private API processing, hosted checkout, or a shareable link checkout; the ID and reference remain unchanged.

```csharp
string paymentJson = await client.Payments.CreateAsync(
    "{\"amount_minor\":50000,\"currency\":\"MZN\",\"reference\":\"ORDER-10492\"}",
    "ORDER-10492-CREATE");
string paymentId = /* read data.id from paymentJson */;

// Choose a mode for this parent payment:
string checkoutJson = await client.Payments.PublicCheckoutAsync(paymentId, "{}", "ORDER-10492-PUBLIC-CHECKOUT");
// Or call PrivateCheckoutAsync(paymentId, payload, key) or LinkCheckoutAsync(paymentId, payload, key).
// Pass the returned token to the buyer-facing page; never expose partner keys.
```

`PrivateCheckoutAsync` submits a method from your backend. `PublicCheckoutAsync` returns an opaque token for hosted checkout. `LinkCheckoutAsync` returns a shareable token for this same payment. `ListCheckoutsAsync(paymentId)` lists private executions, public/link executions, and sessions where no method was submitted; each execution keeps its originating checkout mode. `RetrieveAsync(paymentId)` fetches status from the API; use verified webhooks for asynchronous updates.

### Synchronous PIN confirmation

For a method that uses a PIN prompt (currently M-Pesa and e-Mola), the provider's 60-second PIN window starts only after the Master dispatches the charge. The API's 80-second `private-checkout` deadline starts when it receives the request, so validation, queueing, and dispatch latency already consume that time. Other gateway methods can use different authorization flows and timings. The API returns HTTP `200` with a terminal attempt or `202` if the result is still pending, processing, or unknown when the deadline expires. The SDK transport timeout defaults to 90 seconds; keep a custom timeout above 80 seconds. If the caller disconnects, durable processing continues; configured webhook endpoints receive the event when the payment result is confirmed. You can also query `GET /v1/payments/{id}` and `GET /v1/payments/{id}/checkouts`. Do not resubmit while the attempt is pending or unknown.

## Function reference

Each row tells you what the call does and when to use it. Partner calls use the API key pair; public catalog and buyer checkout calls do not. Supply a stable `Idempotency-Key` for writes where required.

| Resource | Function | Endpoint | What it does / when to use it |
|---|---|---|---|
| Payments | `Payments.CreateAsync` | `POST /v1/payments` | Creates the parent payment only. Save its ID and reference, then choose a checkout mode. |
| Payments | `Payments.PrivateCheckoutAsync` | `POST /v1/payments/{id}/private-checkout` | Submits a selected method from your server for the existing payment. Use when your backend controls the interface. |
| Payments | `Payments.PublicCheckoutAsync` | `POST /v1/payments/{id}/public-checkout` | Creates an opaque token for EDPay-hosted checkout on the existing payment. |
| Payments | `Payments.LinkCheckoutAsync` | `POST /v1/payments/{id}/link-checkout` | Creates a shareable buyer token for the existing payment; it does not create another payment. |
| Payments | `Payments.ListCheckoutsAsync` | `GET /v1/payments/{id}/checkouts` | Lists executions with their checkout mode and sessions where no method was submitted. |
| Payments | `Payments.RetrieveAsync` | `GET /v1/payments/{id}` | Retrieves remote payment status. |
| Payments | `Payments.EventsAsync` | `GET /v1/payments/{id}/events` | Reads recorded payment status changes. |
| Payments | `Payments.ListAsync` | `GET /v1/payments` | Lists account payments for reconciliation or history. |
| Hosted checkout | `Payments.GetCheckoutAsync` | `GET /v1/payments/checkouts/{token}` | Loads the buyer-facing hosted or link checkout with its opaque token. |
| Hosted checkout | `Payments.SubmitCheckoutPaymentAsync` | `POST /v1/payments/checkouts/{token}/payment` | Submits the buyer-selected method and required method fields with an idempotency key. |
| Hosted checkout | `Payments.RetrieveCheckoutPaymentAsync` | `GET /v1/payments/checkouts/{token}/payment` | Reads the parent payment status without exposing its ID to the buyer. |
| Hosted checkout | `Payments.FramePolicyAsync` | `GET /v1/payments/checkouts/{token}/frame-policy` | Checks whether the hosted checkout can be embedded. |
| Customers | `Customers.CreateAsync` | `POST /v1/customers` | Creates a customer record for the authenticated account. Save customer details before associating them with payments or invoices. |
| Customers | `Customers.ListAsync` | `GET /v1/customers` | Lists customers in the current account and environment. Build a customer picker or reconcile customer records. |
| Customers | `Customers.RetrieveAsync` | `GET /v1/customers/{id}` | Reads one customer record. Load a customer's saved details by ID. |
| Customers | `Customers.UpdateAsync` | `PATCH /v1/customers/{id}` | Updates the supplied customer fields. Correct or refresh a customer's details; use a stable idempotency key where required. |
| Invoices | `Invoices.CreateAsync` | `POST /v1/invoices` | Creates a draft invoice with its customer and line items. Prepare an invoice before issuing it to the customer. |
| Invoices | `Invoices.ListAsync` | `GET /v1/invoices` | Lists invoices, optionally limited to a page size. Show or reconcile invoices for the account. |
| Invoices | `Invoices.RetrieveAsync` | `GET /v1/invoices/{id}` | Reads one invoice and its current state. Load invoice details or check its lifecycle. |
| Invoices | `Invoices.FinalizeAsync` | `POST /v1/invoices/{id}/finalize` | Issues the draft invoice and makes it available for payment. The invoice is complete and ready to send to the customer. |
| Invoices | `Invoices.VoidAsync` | `POST /v1/invoices/{id}/void` | Voids an invoice that should no longer be payable. Cancel an issued invoice according to its allowed state transitions. |
| Invoice series | `Subscriptions.CreateAsync` | `POST /v1/invoices/series` | Creates a schedule that issues invoices on the configured interval. Bill a customer repeatedly using a defined start date, interval, and due period. |
| Invoice series | `Subscriptions.ListAsync` | `GET /v1/invoices/series` | Lists invoice series; there is no series-detail endpoint in this API version. Review recurring schedules for the account. |
| Invoice series | `Subscriptions.PauseAsync` | `POST /v1/invoices/series/{id}/pause` | Pauses future invoice generation for a series. Temporarily stop recurring billing without deleting the schedule. |
| Invoice series | `Subscriptions.ResumeAsync` | `POST /v1/invoices/series/{id}/resume` | Resumes invoice generation for a paused series. Restart recurring billing after the pause condition has ended. |
| FX quotes | `Fx.QuoteAsync` | `GET /v1/fx/quote` | Returns an indicative conversion quote for an amount and currency pair. Display a conversion estimate before creating a payment. A quote is not a settlement guarantee. |
| Wallets | `Wallets.ListAsync` | `GET /v1/wallets` | Lists wallets available to the authenticated account and environment. Choose a wallet before reading its balance or entries. |
| Wallets | `Wallets.RetrieveAsync` | `GET /v1/wallets/{id}` | Reads one wallet's details. Load the wallet metadata for an account view. |
| Wallets | `Wallets.BalanceAsync` | `GET /v1/wallets/{id}/balance` | Reads the wallet's current balance. Show the available or pending balance for a wallet. |
| Wallets | `Wallets.EntriesAsync` | `GET /v1/wallets/{id}/entries` | Lists ledger entries with cursor pagination. Show a transaction statement; pass the returned cursor for the next page. |
| Webhooks | `Webhooks.ListEndpointsAsync` | `GET /v1/webhooks/endpoints` | Lists configured webhook destinations. Review which event destinations are active. |
| Webhooks | `Webhooks.CreateEndpointAsync` | `POST /v1/webhooks/endpoints` | Registers an HTTPS receiver and selected event types. The signing secret is shown only at creation. Connect your server to receive payment lifecycle events; store the returned secret securely. |
| Webhooks | `Webhooks.UpdateEndpointAsync` | `PATCH /v1/webhooks/endpoints/{id}` | Updates a destination's URL or subscribed event types. Change a receiver without creating a second endpoint. |
| Webhooks | `Webhooks.DisableEndpointAsync` | `POST /v1/webhooks/endpoints/{id}/disable` | Stops future deliveries to an endpoint while retaining its record. Temporarily stop a destination while investigating or migrating it. |
| Webhooks | `Webhooks.DeleteEndpointAsync` | `DELETE /v1/webhooks/endpoints/{id}` | Removes a webhook endpoint. Permanently remove a destination that is no longer used. |
| Webhooks | `Webhooks.ListDeliveriesAsync` | `GET /v1/webhooks/deliveries` | Lists delivery attempts and their summary states. Monitor failed, pending, or completed event deliveries. |
| Webhooks | `Webhooks.RetrieveDeliveryAsync` | `GET /v1/webhooks/deliveries/{id}` | Reads one delivery's status and attempt metadata. Inspect one delivery before deciding whether to retry it. |
| Webhooks | `Webhooks.RetryDeliveryAsync` | `POST /v1/webhooks/deliveries/{id}/retry` | Queues a retry for an eligible failed delivery. Retry after fixing the receiver; the endpoint must be active. |
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
