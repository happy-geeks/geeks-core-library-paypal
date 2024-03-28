using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class LineItemModel
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonProperty("upc")]
    [JsonPropertyName("upc")]
    public UpcModel Upc { get; set; }
    [JsonProperty("unit_amount")]
    [JsonPropertyName("unit_amount")]
    public AmountModel UnitAmount { get; set; }
    [JsonProperty("tax")]
    [JsonPropertyName("tax")]
    public AmountModel Tax { get; set; }
    [JsonProperty("discount_amount")]
    [JsonPropertyName("discount_amount")]
    public AmountModel DiscountAmount { get; set; }
    [JsonProperty("total_amount")]
    [JsonPropertyName("total_amount")]
    public AmountModel TotalAmount { get; set; }
    [JsonProperty("unit_of_measure")]
    [JsonPropertyName("unit_of_measure")]
    public string UnitOfMeasure { get; set; }
    [JsonProperty("quantity")]
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
    [JsonProperty("commodity_code")]
    [JsonPropertyName("commodity_code")]
    public string CommodityCode { get; set; }
}