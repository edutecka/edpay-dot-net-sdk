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

var payload = """
    {"amount_minor":50000,"currency":"MZN",
     "method":"method_from_payment_methods",
     "phone":"+25884xxxxxxx",
     "reference":"ORDER-10492","description":"Example order"}
    """;

string responseJson = await client.Payments.CreateAsync(
    payload,
    idempotencyKey: "ORDER-10492");
Console.WriteLine(responseJson);
```

Replace `method_from_payment_methods` with an ID returned by `GET /v1/public/payment-methods`. `amount_minor` uses minor currency units; `50000` means MZN 500.00. Include `phone` when required by the selected method. API keys determine the environment. `BaseUrl` sets the API host. The client sends `X-EDPay-Public-Key`, `X-EDPay-Private-Key`, and the optional `Idempotency-Key` headers.

## Hosted checkout and webhooks

Create a hosted checkout session with `client.Checkout.CreateSessionAsync(jsonPayload, idempotencyKey)`. The checkout methods accept and return JSON text. Redirect the customer to the URL returned by the API and confirm the final payment server-side.

Verify webhook signatures against the unmodified request body before processing an event:

```csharp
using EDPay.Sdk.Webhooks;

bool isValid = WebhookVerifier.Verify(rawRequestBody, signatureHeader, webhookSecret);
if (!isValid)
{
    // Reject the callback; do not process its event.
}
```

Pass the received `X-EDPay-Signature` header value and the unmodified request body to the verifier. Keep the webhook secret in protected server-side configuration.

## Available operations

- Payments: `CreateAsync`, `RetrieveAsync`, `CancelAsync`
- Checkout sessions: `CreateSessionAsync`, `RetrieveSessionAsync`, `ExpireSessionAsync`
- Invoices: `CreateAsync`, `ListAsync`, `RetrieveAsync`, `FinalizeAsync`, `VoidAsync`
- Subscriptions: `CreateAsync`, `ListAsync`, `RetrieveAsync`, `PauseAsync`, `ResumeAsync`, `CancelAsync`
- Webhooks: `WebhookVerifier.Verify`

Resource methods return JSON text. Deserialize it with `System.Text.Json` to access response fields such as payment ID, status, and checkout URL.

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
