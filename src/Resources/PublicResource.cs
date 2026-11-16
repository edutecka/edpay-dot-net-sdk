using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class PublicResource
    {
        private readonly EDPayClient _client;
        public PublicResource(EDPayClient client) { _client = client; }
        public Task<string> ListPaymentMethodsAsync() => _client.SendPublicRequestAsync(HttpMethod.Get, "/v1/public/payment-methods");
        /// <summary>Posts the public support-chat request and returns its server-sent event stream as text.</summary>
        public Task<string> SupportChatAsync(string jsonPayload) => _client.SendPublicRequestAsync(HttpMethod.Post, "/v1/public/support/chat", jsonPayload);
    }
}
