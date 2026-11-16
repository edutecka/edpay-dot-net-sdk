using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EDPay.Sdk.Webhooks
{
    /// <summary>Verifies EDPay timestamped Payment-Signature webhook headers.</summary>
    public static class WebhookVerifier
    {
        public static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(5);
        private static readonly Regex SignatureHeader = new Regex(@"^t=([0-9]+),v1=([0-9a-fA-F]{64})$", RegexOptions.CultureInvariant);

        public static bool Verify(string payload, string signatureHeader, string secret)
        {
            return payload != null && Verify(Encoding.UTF8.GetBytes(payload), signatureHeader, secret);
        }

        public static bool Verify(byte[] rawBody, string signatureHeader, string secret)
        {
            return Verify(rawBody, signatureHeader, secret, DateTimeOffset.UtcNow, DefaultTolerance);
        }

        public static bool Verify(byte[] rawBody, string signatureHeader, string secret, TimeSpan tolerance)
        {
            return Verify(rawBody, signatureHeader, secret, DateTimeOffset.UtcNow, tolerance);
        }

        /// <summary>The explicit clock overload supports deterministic validation and synchronized-clock applications.</summary>
        public static bool Verify(byte[] rawBody, string signatureHeader, string secret, DateTimeOffset now, TimeSpan tolerance)
        {
            if (rawBody == null || string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(secret) || tolerance < TimeSpan.Zero)
                return false;

            var match = SignatureHeader.Match(signatureHeader.Trim());
            if (!match.Success || !long.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var timestamp))
                return false;

            try
            {
                var signedAt = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                if ((now - signedAt).Duration() > tolerance)
                    return false;

                var signedPrefix = Encoding.ASCII.GetBytes(match.Groups[1].Value + ".");
                var signedPayload = new byte[signedPrefix.Length + rawBody.Length];
                Buffer.BlockCopy(signedPrefix, 0, signedPayload, 0, signedPrefix.Length);
                Buffer.BlockCopy(rawBody, 0, signedPayload, signedPrefix.Length, rawBody.Length);

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
                var expected = hmac.ComputeHash(signedPayload);
                var supplied = ParseHex(match.Groups[2].Value);
#if NETSTANDARD2_0
                if (expected.Length != supplied.Length) return false;
                int result = 0;
                for (int i = 0; i < expected.Length; i++) result |= expected[i] ^ supplied[i];
                return result == 0;
#else
                return CryptographicOperations.FixedTimeEquals(expected, supplied);
#endif
            }
            catch
            {
                return false;
            }
        }

        private static byte[] ParseHex(string value)
        {
            var bytes = new byte[value.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return bytes;
        }
    }
}
