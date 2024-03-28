using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PaymentSourceModel
{
    [JsonProperty("paypal")]
    [JsonPropertyName("paypal")]
    public PaymentSourcePayPalModel PayPal { get; set; }
    [JsonProperty("resource")]
    [JsonPropertyName("resource")]
    public PaymentSourcePayPalModel Resource { get; set; }
}