using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EDPay.Sdk.Exceptions;
using EDPay.Sdk.Resources;

namespace EDPay.Sdk
{
    public enum EDPayEnvironment
    {
        Production,
        Sandbox
    }

    public class EDPayOptions
    {
        public required string PublicKey { get; set; }
        public required string PrivateKey { get; set; }
        public EDPayEnvironment Environment { get; set; } = EDPayEnvironment.Production;
        public string? BaseUrl { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    }

    public class EDPayClient
    {
        private readonly EDPayOptions _options;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PaymentsResource Payments { get; }
        public CheckoutResource Checkout { get; }
        public InvoicesResource Invoices { get; }
        public SubscriptionsResource Subscriptions { get; }
        public CustomersResource Customers { get; }
        public PaymentLinksResource PaymentLinks { get; }
        public WalletsResource Wallets { get; }
        public FxResource Fx { get; }
        public WebhooksResource Webhooks { get; }
        public PublicResource Public { get; }

        public EDPayClient(EDPayOptions options, HttpClient? httpClient = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = options.Timeout;

            _baseUrl = !string.IsNullOrWhiteSpace(options.BaseUrl)
                ? options.BaseUrl.TrimEnd('/')
                : (options.Environment == EDPayEnvironment.Sandbox
                    ? "https://api-pay.edutecka.com"
                    : "https://api-pay.edutecka.com");

            Payments = new PaymentsResource(this);
            Checkout = new CheckoutResource(this);
            Invoices = new InvoicesResource(this);
            Subscriptions = new SubscriptionsResource(this);
            Customers = new CustomersResource(this);
            PaymentLinks = new PaymentLinksResource(this);
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
            request.Headers.Add("User-Agent", "EDPay-DotNet-SDK/1.5.0");

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
