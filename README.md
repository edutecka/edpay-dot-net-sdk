# EDPay .NET SDK

[![NuGet](https://img.shields.io/nuget/v/EDPay.Sdk.svg)](https://www.nuget.org/packages/EDPay.Sdk/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

SDK oficial da EDPay para a plataforma .NET (C#, ASP.NET Core, Blazor, worker services e aplicações MAUI).

---

## Instalação

```bash
dotnet add package EDPay.Sdk
```

---

## Inicialização

```csharp
using EDPay.Sdk;

var client = new EDPayClient(new EDPayOptions
{
    PublicKey = "pk_live_...",
    PrivateKey = "sk_live_...",
    Environment = EDPayEnvironment.Production // ou Sandbox
});
```

---

## Criar um Pagamento Direto (API Checkout)

```csharp
var payment = await client.Payments.CreateAsync(new PaymentCreateRequest
{
    AmountMinor = 50000,           // 500.00 MZN
    Currency = "MZN",
    Method = "emola",              // ou "mpesa", "card", "apple_pay", etc.
    Phone = "+25884xxxxxxx",
    Reference = "FAT-2026-001",
    Description = "Subscrição Mensal",
    EmolaCustomMessage = "Pagamento de Serviços via EDPay"
});

Console.WriteLine($"ID: {payment.Id}");
Console.WriteLine($"Status: {payment.Status}");
```

---

## Verificação de Webhooks no ASP.NET Core

```csharp
using EDPay.Sdk.Webhooks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/webhooks/edpay")]
public class EdpayWebhookController : ControllerBase
{
    private readonly string _webhookSecret = "whsec_...";

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var rawPayload = await reader.ReadToEndAsync();
        var signature = Request.Headers["X-EDPay-Signature"].ToString();

        if (!WebhookVerifier.Verify(rawPayload, signature, _webhookSecret))
        {
            return BadRequest(new { error = "Assinatura inválida" });
        }

        // Processar evento
        return Ok(new { received = true });
    }
}
```

---

## Condições Comerciais

- Tarifa unificada de 6,00% em Meticais (MZN).
- Liquidação bancária direta para a conta do comerciante de 2 a 7 dias úteis.

---

## Licença

MIT © [EDPay](https://edpay.co.mz)
