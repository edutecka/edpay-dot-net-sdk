using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class FxResource
    {
        private readonly EDPayClient _client;
        public FxResource(EDPayClient client) { _client = client; }
        public Task<string> QuoteAsync(string fromCurrency, string toCurrency, long amountMinor)
        {
            if (amountMinor < 1) throw new ArgumentOutOfRangeException(nameof(amountMinor), "amountMinor must be positive");
            var query = $"from_currency={Uri.EscapeDataString(fromCurrency)}&to_currency={Uri.EscapeDataString(toCurrency)}&amount_minor={amountMinor}";
            return _client.SendRequestAsync(HttpMethod.Get, "/v1/fx/quote?" + query);
        }
    }
}
