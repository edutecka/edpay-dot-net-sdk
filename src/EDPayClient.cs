using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EDPay.Sdk.Exceptions;
using EDPay.Sdk.Resources;

namespace EDPay.Sdk
{
    public class EDPayOptions
    {
        public required string PublicKey { get; set; }
        public required string PrivateKey { get; set; }
        public string? BaseUrl { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(90);
    }

    public class EDPayClient
    {
        private readonly EDPayOptions _options;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PaymentsResource Payments { get; }
        public InvoicesResource Invoices { get; }
        public SubscriptionsResource Subscriptions { get; }
        public CustomersResource Customers { get; }
        public WalletsResource Wallets { get; }
        public FxResource Fx { get; }
        public WebhooksResource Webhooks { get; }
        public PublicResource Public { get; }

        public EDPayClient(EDPayOptions options, HttpClient? httpClient = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            var publicKey = options.PublicKey?.Trim();
            var privateKey = options.PrivateKey?.Trim();
            var publicMode = System.Text.RegularExpressions.Regex.Match(publicKey ?? string.Empty, @"^pk_(test|live)_[0-9a-fA-F]{24}$").Groups[1].Value;
            var privateMode = System.Text.RegularExpressions.Regex.Match(privateKey ?? string.Empty, @"^sk_(test|live)_[0-9a-fA-F]{64}$").Groups[1].Value;
            if (string.IsNullOrEmpty(publicMode) || string.IsNullOrEmpty(privateMode))
                throw new ArgumentException("A valid pk_test_/pk_live_ PublicKey (24 hex characters) and sk_test_/sk_live_ PrivateKey (64 hex characters) are required.", nameof(options));
            if (publicMode != privateMode)
                throw new ArgumentException("EDPay PublicKey and PrivateKey must use the same environment (test or live).", nameof(options));
            options.PublicKey = publicKey;
            options.PrivateKey = privateKey;
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = options.Timeout;

            _baseUrl = !string.IsNullOrWhiteSpace(options.BaseUrl)
                ? options.BaseUrl.TrimEnd('/')
                : "https://api-pay.edutecka.com";

            Payments = new PaymentsResource(this);
            Invoices = new InvoicesResource(this);
            Subscriptions = new SubscriptionsResource(this);
            Customers = new CustomersResource(this);
            Wallets = new WalletsResource(this);
            Fx = new FxResource(this);
            Webhooks = new WebhooksResource(this);
            Public = new PublicResource(this);
        }

        public async Task<string> SendRequestAsync(HttpMethod method, string endpoint, string? jsonPayload = null, string? idempotencyKey = null)
        {
            return await SendRequestCoreAsync(method, endpoint, jsonPayload, idempotencyKey, true);
        }

        public async Task<string> SendPublicRequestAsync(HttpMethod method, string endpoint, string? jsonPayload = null, string? idempotencyKey = null)
        {
            return await SendRequestCoreAsync(method, endpoint, jsonPayload, idempotencyKey, false);
        }

        private async Task<string> SendRequestCoreAsync(HttpMethod method, string endpoint, string? jsonPayload, string? idempotencyKey, bool authenticated)
        {
            using var request = new HttpRequestMessage(method, $"{_baseUrl}{endpoint}");
            if (authenticated)
            {
                request.Headers.Add("X-EDPay-Public-Key", _options.PublicKey);
                request.Headers.Add("X-EDPay-Private-Key", _options.PrivateKey);
            }
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "EDPay-DotNet-SDK/1.6.2");

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                request.Headers.Add("Idempotency-Key", idempotencyKey);
            }

            if (jsonPayload != null)
            {
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                response.Headers.TryGetValues("X-Request-Id", out var requestIds);
                response.Headers.TryGetValues("Retry-After", out var retryAfterValues);
                throw new EDPayException(
                    $"EDPay API error: {(int)response.StatusCode}",
                    (int)response.StatusCode,
                    content,
                    requestIds?.FirstOrDefault(),
                    retryAfterValues?.FirstOrDefault());
            }

            return content;
        }
    }
}
