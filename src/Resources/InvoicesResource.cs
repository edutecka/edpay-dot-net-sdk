using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class InvoicesResource
    {
        private readonly EDPayClient _client;

        public InvoicesResource(EDPayClient client)
        {
            _client = client;
        }

        public Task<string> CreateAsync(string jsonPayload, string? idempotencyKey = null)
        {
            return _client.SendRequestAsync(HttpMethod.Post, "/v1/invoices", jsonPayload, idempotencyKey);
        }

        public Task<string> ListAsync(string? queryParams = null)
        {
            var path = "/v1/invoices" + (!string.IsNullOrWhiteSpace(queryParams) ? $"?{queryParams}" : "");
            return _client.SendRequestAsync(HttpMethod.Get, path);
        }

        public Task<string> RetrieveAsync(string invoiceId)
        {
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/invoices/{invoiceId}");
        }

        public Task<string> FinalizeAsync(string invoiceId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/invoices/{invoiceId}/finalize", "{}");
        }

        public Task<string> VoidAsync(string invoiceId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/invoices/{invoiceId}/void", "{}");
        }
    }
}
