using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class BreakdownModel
{
    [JsonProperty("item_total")]
    [JsonPropertyName("item_total")]
    public AmountModel ItemTotal { get; set; } = new();

    [JsonProperty("tax_total")]
    [JsonPropertyName("tax_total")]
    public AmountModel TaxTotal { get; set; } = new();

    [JsonProperty("shipping")]
    [JsonPropertyName("shipping")]
    public AmountModel Shipping { get; set; } = new();

    [JsonProperty("discount")]
    [JsonPropertyName("discount")]
    public AmountModel Discount { get; set; } = new();

    [JsonProperty("handling")]
    [JsonPropertyName("handling")]
    public AmountModel Handling { get; set; } = new();

    [JsonProperty("insurance")]
    [JsonPropertyName("insurance")]
    public AmountModel Insurance { get; set; } = new();

    [JsonProperty("shipping_discount")]
    [JsonPropertyName("shipping_discount")]
    public AmountModel ShippingDiscount { get; set; } = new();
}