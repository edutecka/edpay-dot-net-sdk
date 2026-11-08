namespace EDPay
{
    public class EDPayClient
    {
        public string PublicKey { get; }
        public string PrivateKey { get; }
        public string BaseUrl { get; }

        public EDPayClient(string publicKey, string privateKey, string baseUrl = "https://api-pay.edutecka.com")
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
            BaseUrl = baseUrl.TrimEnd('/');
        }
    }
}
