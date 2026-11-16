namespace EDPay.Sdk.Webhooks
{
    /// <summary>Standard partner payment webhook payload. PaymentLinkId is null for payments not created from a link.</summary>
    public sealed class WebhookEvent
    {
        public string? id { get; set; }
        public string? @object { get; set; }
        public string? type { get; set; }
        public string? created_at { get; set; }
        public WebhookPaymentData? data { get; set; }
    }

    public sealed class WebhookPaymentData
    {
        public string? @object { get; set; }
        public string? id { get; set; }
        public string? reference { get; set; }
        public string? payment_link_id { get; set; }
        public string? trans_id { get; set; }
        public string? method { get; set; }
        public string? status { get; set; }
        public long amount_minor { get; set; }
        public string? currency { get; set; }
        public long? platform_fee_minor { get; set; }
        public long? receiver_net_minor { get; set; }
        public string? created_at { get; set; }
        public string? reconciliation_status { get; set; }
        public string? reconciliation_code { get; set; }
    }
}
