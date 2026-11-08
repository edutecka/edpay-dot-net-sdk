using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class SubscriptionsResource
    {
        private readonly EDPayClient _client;

        public SubscriptionsResource(EDPayClient client)
        {
            _client = client;
        }

        public Task<string> CreateAsync(string jsonPayload, string? idempotencyKey = null)
        {
            return _client.SendRequestAsync(HttpMethod.Post, "/v1/invoice-series", jsonPayload, idempotencyKey);
        }

        public Task<string> ListAsync(string? queryParams = null)
        {
            var path = "/v1/invoice-series" + (!string.IsNullOrWhiteSpace(queryParams) ? $"?{queryParams}" : "");
            return _client.SendRequestAsync(HttpMethod.Get, path);
        }

        public Task<string> RetrieveAsync(string seriesId)
        {
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/invoice-series/{seriesId}");
        }

        public Task<string> PauseAsync(string seriesId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/invoice-series/{seriesId}/pause", "{}");
        }

        public Task<string> ResumeAsync(string seriesId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/invoice-series/{seriesId}/resume", "{}");
        }

        public Task<string> CancelAsync(string seriesId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/invoice-series/{seriesId}/cancel", "{}");
        }
    }
}
