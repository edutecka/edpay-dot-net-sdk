using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class PaymentsResource
    {
        private readonly EDPayClient _client;
        public PaymentsResource(EDPayClient client) { _client = client; }

        public Task<string> CreateAsync(string jsonPayload, string idempotencyKey) => _client.SendRequestAsync(HttpMethod.Post, "/v1/payments", jsonPayload, idempotencyKey);
        public Task<string> PrivateCheckoutAsync(string paymentId, string jsonPayload, string idempotencyKey) => _client.SendRequestAsync(HttpMethod.Post, $"/v1/payments/{Uri.EscapeDataString(paymentId)}/private-checkout", jsonPayload, idempotencyKey);
        public Task<string> PublicCheckoutAsync(string paymentId, string jsonPayload, string idempotencyKey) => _client.SendRequestAsync(HttpMethod.Post, $"/v1/payments/{Uri.EscapeDataString(paymentId)}/public-checkout", jsonPayload, idempotencyKey);
        public Task<string> LinkCheckoutAsync(string paymentId, string jsonPayload, string idempotencyKey) => _client.SendRequestAsync(HttpMethod.Post, $"/v1/payments/{Uri.EscapeDataString(paymentId)}/link-checkout", jsonPayload, idempotencyKey);
        /// <summary>Lists executions with their checkout mode and sessions with no submission.</summary>
        public Task<string> ListCheckoutsAsync(string paymentId) => _client.SendRequestAsync(HttpMethod.Get, $"/v1/payments/{Uri.EscapeDataString(paymentId)}/checkouts");
        public Task<string> GetCheckoutAsync(string token) => _client.SendPublicRequestAsync(HttpMethod.Get, $"/v1/payments/checkouts/{Uri.EscapeDataString(token)}");
        public Task<string> SubmitCheckoutPaymentAsync(string token, string jsonPayload, string idempotencyKey) => _client.SendPublicRequestAsync(HttpMethod.Post, $"/v1/payments/checkouts/{Uri.EscapeDataString(token)}/payment", jsonPayload, idempotencyKey);
        public Task<string> RetrieveCheckoutPaymentAsync(string token) => _client.SendPublicRequestAsync(HttpMethod.Get, $"/v1/payments/checkouts/{Uri.EscapeDataString(token)}/payment");
        public Task<string> FramePolicyAsync(string token) => _client.SendPublicRequestAsync(HttpMethod.Get, $"/v1/payments/checkouts/{Uri.EscapeDataString(token)}/frame-policy");
        public Task<string> ListAsync(int limit = 50)
        {
            if (limit < 1 || limit > 100) throw new ArgumentOutOfRangeException(nameof(limit));
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/payments?limit={limit}");
        }
        public Task<string> RetrieveAsync(string paymentId) => _client.SendRequestAsync(HttpMethod.Get, $"/v1/payments/{Uri.EscapeDataString(paymentId)}");
        public Task<string> EventsAsync(string paymentId) => _client.SendRequestAsync(HttpMethod.Get, $"/v1/payments/{Uri.EscapeDataString(paymentId)}/events");
    }
}
