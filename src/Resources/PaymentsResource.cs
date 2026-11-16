using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class PaymentsResource
    {
        private readonly EDPayClient _client;

        public PaymentsResource(EDPayClient client)
        {
            _client = client;
        }

        public Task<string> CreateAsync(string jsonPayload, string idempotencyKey)
        {
            return _client.SendRequestAsync(HttpMethod.Post, "/v1/payments", jsonPayload, idempotencyKey);
        }

        public Task<string> CreateCheckoutAsync(string jsonPayload, string idempotencyKey)
        {
            return _client.SendRequestAsync(HttpMethod.Post, "/v1/payment-checkout", jsonPayload, idempotencyKey);
        }

        public Task<string> ListAsync(int limit = 50)
        {
            if (limit < 1 || limit > 100) throw new ArgumentOutOfRangeException(nameof(limit), "limit must be between 1 and 100");
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/payments?limit={limit}");
        }

        public Task<string> RetrieveAsync(string paymentId)
        {
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/payments/{Uri.EscapeDataString(paymentId)}");
        }

        public Task<string> EventsAsync(string paymentId)
        {
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/payments/{Uri.EscapeDataString(paymentId)}/events");
        }

        public Task<string> AttemptsAsync(string paymentId)
        {
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/payments/{Uri.EscapeDataString(paymentId)}/attempts");
        }
    }
}
