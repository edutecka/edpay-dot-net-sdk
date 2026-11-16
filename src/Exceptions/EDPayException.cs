using System;

namespace EDPay.Sdk.Exceptions
{
    public class EDPayException : Exception
    {
        public int StatusCode { get; }
        public string? ResponseBody { get; }
        public string? RequestId { get; }
        public string? RetryAfter { get; }

        public EDPayException(string message, int statusCode = 400, string? responseBody = null, string? requestId = null, string? retryAfter = null)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            RequestId = requestId;
            RetryAfter = retryAfter;
        }
    }
}
