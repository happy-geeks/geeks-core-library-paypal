using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class OrderRequestModel
{
    [JsonProperty("intent")]
    [JsonPropertyName("intent")]
    public string Intent { get; set; }
    [JsonProperty("payment_source")]
    [JsonPropertyName("payment_source")]
    public PaymentSourceModel PaymentSource { get; set; }
    [JsonProperty("purchase_units")]
    [JsonPropertyName("purchase_units")]
    public List<PurchaseUnitModel> PurchaseUnits { get; set; }
}