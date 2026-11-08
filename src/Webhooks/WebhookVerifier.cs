using System;
using System.Security.Cryptography;
using System.Text;

namespace EDPay.Sdk.Webhooks
{
    public static class WebhookVerifier
    {
        public static bool Verify(string payload, string signature, string secret)
        {
            if (string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(secret))
            {
                return false;
            }

            try
            {
                var secretBytes = Encoding.UTF8.GetBytes(secret);
                var payloadBytes = Encoding.UTF8.GetBytes(payload);

                using var hmac = new HMACSHA256(secretBytes);
                var hash = hmac.ComputeHash(payloadBytes);
                var computedHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                var cleanSig = signature.Trim().ToLowerInvariant();

                var computedBytes = Encoding.UTF8.GetBytes(computedHex);
                var sigBytes = Encoding.UTF8.GetBytes(cleanSig);

#if NETSTANDARD2_0
                if (computedBytes.Length != sigBytes.Length) return false;
                int result = 0;
                for (int i = 0; i < computedBytes.Length; i++)
                {
                    result |= computedBytes[i] ^ sigBytes[i];
                }
                return result == 0;
#else
                return CryptographicOperations.FixedTimeEquals(computedBytes, sigBytes);
#endif
            }
            catch
            {
                return false;
            }
        }
    }
}
