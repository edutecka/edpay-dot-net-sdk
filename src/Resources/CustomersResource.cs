using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class CustomersResource
    {
        private readonly EDPayClient _client;
        public CustomersResource(EDPayClient client) { _client = client; }
        public Task<string> CreateAsync(string jsonPayload, string idempotencyKey) => _client.SendRequestAsync(HttpMethod.Post, "/v1/customers", jsonPayload, idempotencyKey);
        public Task<string> ListAsync() => _client.SendRequestAsync(HttpMethod.Get, "/v1/customers");
        public Task<string> RetrieveAsync(string customerId) => _client.SendRequestAsync(HttpMethod.Get, $"/v1/customers/{Uri.EscapeDataString(customerId)}");
        public Task<string> UpdateAsync(string customerId, string jsonPayload, string idempotencyKey) => _client.SendRequestAsync(new HttpMethod("PATCH"), $"/v1/customers/{Uri.EscapeDataString(customerId)}", jsonPayload, idempotencyKey);
    }
}
