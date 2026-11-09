# EDPay .NET SDK

Official .NET client for server-to-server EDPay integrations. The project targets .NET 8, .NET 7, and .NET Standard 2.0 and exposes asynchronous payment, hosted-checkout, invoice, subscription, and webhook-verification components.

## Requirements

- .NET SDK 8 or later to build the multi-targeted project
- A consuming application compatible with one of the target frameworks
- EDPay API keys for the environment you intend to use

## Install from source

No NuGet release is assumed. Build a local package from this repository:

```bash
git clone https://github.com/edutecka/edpay-dot-net-sdk.git
cd edpay-dot-net-sdk
dotnet build
dotnet pack -o ./artifacts
```

In the consuming application's directory, install that locally built package by pointing NuGet at the generated feed:

```bash
dotnet add package EDPay.Sdk --source /path/to/edpay-dot-net-sdk/artifacts
```

Alternatively, add `EDPay.Sdk.csproj` as a project reference in your solution. Rebuild the package after pulling SDK changes.

## Create a payment

Store credentials in environment variables, user secrets, or a managed secret store. `Payments.CreateAsync` accepts a JSON request body and returns the API response as a JSON string; it does not map the response to SDK model classes.

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
    Environment = EDPayEnvironment.Sandbox
});

var payload = """
    {"amount_minor":50000,"currency":"MZN",
     "method":"method_from_payment_methods",
     "reference":"ORDER-10492","description":"Example order"}
    """;

string responseJson = await client.Payments.CreateAsync(
    payload,
    idempotencyKey: "ORDER-10492");
Console.WriteLine(responseJson);
```

Replace `method_from_payment_methods` with the method identifier returned by `GET /v1/public/payment-methods`; the SDK does not hard-code a payment provider or expose that endpoint as a resource. The request body is sent as provided to `POST /v1/payments`. The client selects the sandbox API base URL when `EDPayEnvironment.Sandbox` is set and sends `X-EDPay-Public-Key`, `X-EDPay-Private-Key`, and the optional `Idempotency-Key` headers.

## Hosted checkout and webhooks

`client.Checkout` provides `CreateSessionAsync`, `RetrieveSessionAsync`, and `ExpireSessionAsync` for hosted checkout sessions. These methods also accept and return JSON strings. Redirect the customer to the URL returned by the API and confirm the payment server-side; do not treat a browser redirect as proof of payment.

Verify webhook signatures against the unmodified request body before processing an event:

```csharp
using EDPay.Sdk.Webhooks;

bool isValid = WebhookVerifier.Verify(rawRequestBody, signatureHeader, webhookSecret);
if (!isValid)
{
    // Reject the callback; do not process its event.
}
```

The signature header value is supplied as received from `X-EDPay-Signature`. Keep the webhook secret in protected server-side configuration.

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
