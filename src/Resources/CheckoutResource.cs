using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDPay.Sdk.Resources
{
    public class CheckoutResource
    {
        private readonly EDPayClient _client;

        public CheckoutResource(EDPayClient client)
        {
            _client = client;
        }

        public Task<string> CreateSessionAsync(string jsonPayload, string idempotencyKey)
        {
            return _client.SendRequestAsync(HttpMethod.Post, "/v1/checkout/sessions", jsonPayload, idempotencyKey);
        }

        public Task<string> RetrieveSessionAsync(string token)
        {
            return _client.SendPublicRequestAsync(HttpMethod.Get, $"/v1/checkout/sessions/{Uri.EscapeDataString(token)}");
        }

        public Task<string> GetPaymentLinkAsync(string linkId)
        {
            return _client.SendPublicRequestAsync(HttpMethod.Get, $"/v1/checkout/payment-links/{Uri.EscapeDataString(linkId)}");
        }

        public Task<string> FramePolicyAsync(string token)
        {
            return _client.SendPublicRequestAsync(HttpMethod.Get, $"/v1/checkout/sessions/{Uri.EscapeDataString(token)}/frame-policy");
        }

        public Task<string> CreateSessionPaymentAsync(string token, string jsonPayload, string idempotencyKey)
        {
            return _client.SendPublicRequestAsync(HttpMethod.Post, $"/v1/checkout/sessions/{Uri.EscapeDataString(token)}/payments", jsonPayload, idempotencyKey);
        }

        public Task<string> RetrieveSessionPaymentAsync(string token, string paymentId)
        {
            return _client.SendPublicRequestAsync(HttpMethod.Get, $"/v1/checkout/sessions/{Uri.EscapeDataString(token)}/payments/{Uri.EscapeDataString(paymentId)}");
        }

        /// <summary>Switch method for the same session payment after its latest attempt definitively failed.</summary>
        public Task<string> CreateSessionPaymentAttemptAsync(string token, string paymentId, string jsonPayload, string idempotencyKey)
        {
            return _client.SendPublicRequestAsync(HttpMethod.Post, $"/v1/checkout/sessions/{Uri.EscapeDataString(token)}/payments/{Uri.EscapeDataString(paymentId)}/attempts", jsonPayload, idempotencyKey);
        }

        public Task<string> CreatePaymentLinkPaymentAsync(string linkId, string jsonPayload, string idempotencyKey)
        {
            return _client.SendPublicRequestAsync(HttpMethod.Post, $"/v1/checkout/payment-links/{Uri.EscapeDataString(linkId)}/payments", jsonPayload, idempotencyKey);
        }

        public Task<string> RetrievePaymentLinkPaymentAsync(string linkId, string paymentId)
        {
            return _client.SendPublicRequestAsync(HttpMethod.Get, $"/v1/checkout/payment-links/{Uri.EscapeDataString(linkId)}/payments/{Uri.EscapeDataString(paymentId)}");
        }

        /// <summary>Switch method for the same payment-link payment after its latest attempt definitively failed.</summary>
        public Task<string> CreatePaymentLinkPaymentAttemptAsync(string linkId, string paymentId, string jsonPayload, string idempotencyKey)
        {
            return _client.SendPublicRequestAsync(HttpMethod.Post, $"/v1/checkout/payment-links/{Uri.EscapeDataString(linkId)}/payments/{Uri.EscapeDataString(paymentId)}/attempts", jsonPayload, idempotencyKey);
        }
    }
}
