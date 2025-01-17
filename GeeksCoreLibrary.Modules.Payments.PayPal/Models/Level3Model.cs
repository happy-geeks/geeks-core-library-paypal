using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class Level3Model
{
    [JsonProperty("shipping_amount")]
    [JsonPropertyName("shipping_amount")]
    public AmountModel ShippingAmount { get; set; } = new();

    [JsonProperty("duty_amount")]
    [JsonPropertyName("duty_amount")]
    public AmountModel DutyAmount { get; set; } = new();

    [JsonProperty("discount_amount")]
    [JsonPropertyName("discount_amount")]
    public AmountModel DiscountAmount { get; set; } = new();

    [JsonProperty("shipping_address")]
    [JsonPropertyName("shipping_address")]
    public AddressModel ShippingAddress { get; set; } = new();

    [JsonProperty("ships_from_postal_code")]
    [JsonPropertyName("ships_from_postal_code")]
    public string? ShipsFromPostalCode { get; set; }

    [JsonProperty("line_items")]
    [JsonPropertyName("line_items")]
    public List<LineItemModel> LineItems { get; set; } = [];
}