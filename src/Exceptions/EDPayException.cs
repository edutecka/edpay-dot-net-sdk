using System;

namespace EDPay.Sdk.Exceptions
{
    public class EDPayException : Exception
    {
        public int StatusCode { get; }
        public string? ResponseBody { get; }

        public EDPayException(string message, int statusCode = 400, string? responseBody = null)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
