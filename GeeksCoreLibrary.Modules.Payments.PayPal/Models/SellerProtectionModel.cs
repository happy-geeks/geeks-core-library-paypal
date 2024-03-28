using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class SellerProtectionModel
{
    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonProperty("dispute_categories")]
    [JsonPropertyName("dispute_categories")]
    public List<string> DisputeCategories { get; set; }
}