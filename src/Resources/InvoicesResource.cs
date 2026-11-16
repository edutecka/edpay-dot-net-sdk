using System;
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

        public Task<string> CreateAsync(string jsonPayload, string idempotencyKey)
        {
            return _client.SendRequestAsync(HttpMethod.Post, "/v1/invoices", jsonPayload, idempotencyKey);
        }

        public Task<string> ListAsync()
        {
            return _client.SendRequestAsync(HttpMethod.Get, "/v1/invoices");
        }

        public Task<string> ListAsync(int limit)
        {
            if (limit < 1 || limit > 100) throw new ArgumentOutOfRangeException(nameof(limit), "limit must be between 1 and 100");
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/invoices?limit={limit}");
        }

        public Task<string> RetrieveAsync(string invoiceId)
        {
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/invoices/{Uri.EscapeDataString(invoiceId)}");
        }

        public Task<string> FinalizeAsync(string invoiceId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/invoices/{Uri.EscapeDataString(invoiceId)}/finalize", "{}");
        }

        public Task<string> VoidAsync(string invoiceId)
        {
            return _client.SendRequestAsync(HttpMethod.Post, $"/v1/invoices/{Uri.EscapeDataString(invoiceId)}/void", "{}");
        }
    }
}
