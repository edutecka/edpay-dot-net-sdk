using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class CheckoutResource
    {
        private readonly EDPayClient _client;

        public CheckoutResource(EDPayClient client)
        {
            _client = client;
        }

        public Task<string> CreateSessionAsync(string jsonPayload, string? idempotencyKey = null)
        {
            return _client.SendRequestAsync(HttpMethod.Post, "/v1/checkout/sessions", jsonPayload, idempotencyKey);
        }

        public Task<string> RetrieveSessionAsync(string sessionId)
        {
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/checkout/sessions/{sessionId}");
        }

        public Task<string> ExpireSessionAsync(string sessionId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/checkout/sessions/{sessionId}/expire", "{}");
        }
    }
}
