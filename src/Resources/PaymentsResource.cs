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

        public Task<string> CreateAsync(string jsonPayload, string? idempotencyKey = null)
        {
            return _client.SendRequestAsync(HttpMethod.Post, "/v1/payments", jsonPayload, idempotencyKey);
        }

        public Task<string> RetrieveAsync(string paymentId)
        {
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/payments/{paymentId}");
        }

        public Task<string> CancelAsync(string paymentId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/payments/{paymentId}/cancel", "{}");
        }
    }
}
