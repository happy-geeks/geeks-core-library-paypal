using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class ShippingModel
{
    [JsonProperty("method")]
    [JsonPropertyName("method")]
    public string Method { get; set; }
    [JsonProperty("address")]
    [JsonPropertyName("address")]
    public AddressModel Address { get; set; }
}