using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class SellerReceivableBreakdownModel
{
    [JsonProperty("gross_amount")]
    [JsonPropertyName("gross_amount")]
    public AmountModel GrossAmount { get; set; } = new();

    [JsonProperty("paypal_fee")]
    [JsonPropertyName("paypal_fee")]
    public AmountModel PaypalFee { get; set; } = new();

    [JsonProperty("net_amount")]
    [JsonPropertyName("net_amount")]
    public AmountModel NetAmount { get; set; } = new();
}