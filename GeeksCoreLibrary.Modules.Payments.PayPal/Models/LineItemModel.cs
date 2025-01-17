using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class LineItemModel
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonProperty("upc")]
    [JsonPropertyName("upc")]
    public UpcModel Upc { get; set; } = new();

    [JsonProperty("unit_amount")]
    [JsonPropertyName("unit_amount")]
    public AmountModel UnitAmount { get; set; } = new();

    [JsonProperty("tax")]
    [JsonPropertyName("tax")]
    public AmountModel Tax { get; set; } = new();

    [JsonProperty("discount_amount")]
    [JsonPropertyName("discount_amount")]
    public AmountModel DiscountAmount { get; set; } = new();

    [JsonProperty("total_amount")]
    [JsonPropertyName("total_amount")]
    public AmountModel TotalAmount { get; set; } = new();

    [JsonProperty("unit_of_measure")]
    [JsonPropertyName("unit_of_measure")]
    public string? UnitOfMeasure { get; set; }

    [JsonProperty("quantity")]
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("commodity_code")]
    [JsonPropertyName("commodity_code")]
    public string? CommodityCode { get; set; }
}