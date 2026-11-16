using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class PaymentLinksResource
    {
        private readonly EDPayClient _client;
        public PaymentLinksResource(EDPayClient client) { _client = client; }
        public Task<string> CreateAsync(string jsonPayload, string idempotencyKey) => _client.SendRequestAsync(HttpMethod.Post, "/v1/payment-links", jsonPayload, idempotencyKey);
        public Task<string> ListAsync() => _client.SendRequestAsync(HttpMethod.Get, "/v1/payment-links");
        public Task<string> RetrieveAsync(string linkId) => _client.SendRequestAsync(HttpMethod.Get, $"/v1/payment-links/{Uri.EscapeDataString(linkId)}");
        public Task<string> DeactivateAsync(string linkId, string idempotencyKey) => _client.SendRequestAsync(HttpMethod.Post, $"/v1/payment-links/{Uri.EscapeDataString(linkId)}/deactivate", "{}", idempotencyKey);
        public Task<string> CreateCheckoutSessionAsync(string jsonPayload, string idempotencyKey) => _client.SendRequestAsync(HttpMethod.Post, "/v1/checkout/sessions", jsonPayload, idempotencyKey);
    }
}
