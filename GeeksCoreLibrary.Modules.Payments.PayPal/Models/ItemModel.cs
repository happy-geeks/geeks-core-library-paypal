using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class ItemModel
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonProperty("sku")]
    [JsonPropertyName("sku")]
    public string Sku { get; set; }
    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonProperty("unit_amount")]
    [JsonPropertyName("unit_amount")]
    public AmountModel UnitAmount { get; set; }
    [JsonProperty("tax")]
    [JsonPropertyName("tax")]
    public AmountModel Tax { get; set; }
    [JsonProperty("quantity")]
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
    [JsonProperty("category")]
    [JsonPropertyName("category")]
    public string Category { get; set; }
}