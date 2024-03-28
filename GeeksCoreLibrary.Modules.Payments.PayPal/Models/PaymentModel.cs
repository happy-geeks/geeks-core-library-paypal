using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PaymentModel
{
    [JsonProperty("parent_payment")]
    [JsonPropertyName("parent_payment")]
    public string ParentPayment { get; set; }
    [JsonProperty("update_time")]
    [JsonPropertyName("update_time")]
    public DateTimeOffset UpdateTime { get; set; }
    [JsonProperty("amount")]
    [JsonPropertyName("amount")]
    public AmountModel Amount { get; set; }
    [JsonProperty("payment_mode")]
    [JsonPropertyName("payment_mode")]
    public string PaymentMode { get; set; }
    [JsonProperty("create_time")]
    [JsonPropertyName("create_time")]
    public DateTimeOffset CreateTime { get; set; }
    [JsonProperty("protection_eligibility")]
    [JsonPropertyName("protection_eligibility")]
    public string ProtectionEligibility { get; set; }
    [JsonProperty("links")]
    [JsonPropertyName("links")]
    public List<LinkModel> Links { get; set; }
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonProperty("state")]
    [JsonPropertyName("state")]
    public string State { get; set; }
    [JsonProperty("payment_source")]
    [JsonPropertyName("payment_source")]
    public PaymentSourceModel PaymentSource { get; set; }
}