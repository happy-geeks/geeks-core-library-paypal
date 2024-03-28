using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class CaptureModel
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonProperty("amount")]
    [JsonPropertyName("amount")]
    public AmountModel Amount { get; set; }
    [JsonProperty("seller_protection")]
    [JsonPropertyName("seller_protection")]
    public SellerProtectionModel SellerProtection { get; set; }
    [JsonProperty("final_capture")]
    [JsonPropertyName("final_capture")]
    public bool FinalCapture { get; set; }
    [JsonProperty("seller_receivable_breakdown")]
    [JsonPropertyName("seller_receivable_breakdown")]
    public SellerReceivableBreakdownModel SellerReceivableBreakdown { get; set; }
    [JsonProperty("create_time")]
    [JsonPropertyName("create_time")]
    public DateTimeOffset CreateTime { get; set; }
    [JsonProperty("update_time")]
    [JsonPropertyName("update_time")]
    public DateTimeOffset UpdateTime { get; set; }
    [JsonProperty("links")]
    [JsonPropertyName("links")]
    public List<LinkModel> Links { get; set; }
}