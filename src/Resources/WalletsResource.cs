using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class WalletsResource
    {
        private readonly EDPayClient _client;
        public WalletsResource(EDPayClient client) { _client = client; }
        public Task<string> ListAsync() => _client.SendRequestAsync(HttpMethod.Get, "/v1/wallets");
        public Task<string> RetrieveAsync(string walletId) => _client.SendRequestAsync(HttpMethod.Get, $"/v1/wallets/{Uri.EscapeDataString(walletId)}");
        public Task<string> BalanceAsync(string walletId) => _client.SendRequestAsync(HttpMethod.Get, $"/v1/wallets/{Uri.EscapeDataString(walletId)}/balance");
        public Task<string> EntriesAsync(string walletId, string? cursor = null, int? limit = null)
        {
            if (limit.HasValue && (limit.Value < 1 || limit.Value > 100)) throw new ArgumentOutOfRangeException(nameof(limit), "limit must be between 1 and 100");
            var query = cursor == null ? "" : "?cursor=" + Uri.EscapeDataString(cursor);
            if (limit.HasValue) query += (query.Length == 0 ? "?" : "&") + "limit=" + limit.Value;
            return _client.SendRequestAsync(HttpMethod.Get, $"/v1/wallets/{Uri.EscapeDataString(walletId)}/entries{query}");
        }
    }
}
