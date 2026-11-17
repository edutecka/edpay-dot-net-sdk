using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class WebhooksResource
    {
        private readonly EDPayClient _client;
        public WebhooksResource(EDPayClient client) { _client = client; }
        public Task<string> ListEndpointsAsync() => _client.SendRequestAsync(HttpMethod.Get, "/v1/webhooks/endpoints");
        public Task<string> CreateEndpointAsync(string jsonPayload, string idempotencyKey) => _client.SendRequestAsync(HttpMethod.Post, "/v1/webhooks/endpoints", jsonPayload, idempotencyKey);
        public Task<string> UpdateEndpointAsync(string endpointId, string jsonPayload) => _client.SendRequestAsync(new HttpMethod("PATCH"), $"/v1/webhooks/endpoints/{Uri.EscapeDataString(endpointId)}", jsonPayload);
        public Task<string> DisableEndpointAsync(string endpointId) => _client.SendRequestAsync(HttpMethod.Post, $"/v1/webhooks/endpoints/{Uri.EscapeDataString(endpointId)}/disable", "{}");
        public Task<string> DeleteEndpointAsync(string endpointId) => _client.SendRequestAsync(HttpMethod.Delete, $"/v1/webhooks/endpoints/{Uri.EscapeDataString(endpointId)}");
        public Task<string> ListDeliveriesAsync(int limit = 50)
        {
            if (limit < 1 || limit > 100) throw new ArgumentOutOfRangeException(nameof(limit), "limit must be between 1 and 100");
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/webhooks/deliveries?limit={limit}");
        }
        public Task<string> RetrieveDeliveryAsync(string deliveryId) => _client.SendRequestAsync(HttpMethod.Get, $"/v1/webhooks/deliveries/{Uri.EscapeDataString(deliveryId)}");
        public Task<string> RetryDeliveryAsync(string deliveryId) => _client.SendRequestAsync(HttpMethod.Post, $"/v1/webhooks/deliveries/{Uri.EscapeDataString(deliveryId)}/retry", "{}");
    }
}
