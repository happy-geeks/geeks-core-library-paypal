using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class OrderResponseModel
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonProperty("intent")]
    [JsonPropertyName("intent")]
    public string Intent { get; set; }
    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonProperty("purchase_units")]
    [JsonPropertyName("purchase_units")]
    public List<PurchaseUnitModel> PurchaseUnits { get; set; }
    [JsonProperty("create_time")]
    [JsonPropertyName("create_time")]
    public DateTimeOffset CreateTime { get; set; }
    [JsonProperty("update_time")]
    [JsonPropertyName("update_time")]
    public DateTimeOffset UpdateTime { get; set; }
    [JsonProperty("links")]
    [JsonPropertyName("links")]
    public List<LinkModel> Links { get; set; }
    [JsonProperty("gross_amount")]
    [JsonPropertyName("gross_amount")]
    public AmountModel GrossAmount { get; set; }
    [JsonProperty("payer")]
    [JsonPropertyName("payer")]
    public PayerModel Payer { get; set; }
    [JsonProperty("payment_source")]
    [JsonPropertyName("payment_source")]
    public PaymentSourceModel PaymentSource { get; set; }
}