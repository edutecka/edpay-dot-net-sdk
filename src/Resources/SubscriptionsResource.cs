using System;
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

        public Task<string> CreateAsync(string jsonPayload, string idempotencyKey)
        {
            return _client.SendRequestAsync(HttpMethod.Post, "/v1/invoices/series", jsonPayload, idempotencyKey);
        }

        public Task<string> ListAsync()
        {
            return _client.SendRequestAsync(HttpMethod.Get, "/v1/invoices/series");
        }

        public Task<string> ListAsync(int limit)
        {
            if (limit < 1 || limit > 100) throw new ArgumentOutOfRangeException(nameof(limit), "limit must be between 1 and 100");
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/invoices/series?limit={limit}");
        }

        public Task<string> PauseAsync(string seriesId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/invoices/series/{Uri.EscapeDataString(seriesId)}/pause", "{}");
        }

        public Task<string> ResumeAsync(string seriesId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/invoices/series/{Uri.EscapeDataString(seriesId)}/resume", "{}");
        }
    }
}
