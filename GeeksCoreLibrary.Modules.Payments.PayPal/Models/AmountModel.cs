using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class AmountModel
{
    [JsonProperty("currency_code")]
    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; }
    [JsonProperty("value")]
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
    [JsonProperty("breakdown")]
    [JsonPropertyName("breakdown")]
    public BreakdownModel Breakdown { get; set; }
    
}