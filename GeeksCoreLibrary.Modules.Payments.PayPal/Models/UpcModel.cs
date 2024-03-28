using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class UpcModel
{
    [JsonProperty("type")]
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonProperty("code")]
    [JsonPropertyName("code")]
    public string Code { get; set; }
}