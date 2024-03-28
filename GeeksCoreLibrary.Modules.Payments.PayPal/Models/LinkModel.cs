using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class LinkModel
{
    [JsonProperty("href")]
    [JsonPropertyName("href")]
    public string Href { get; set; }
    [JsonProperty("rel")]
    [JsonPropertyName("rel")]
    public string Rel { get; set; }
    [JsonProperty("method")]
    [JsonPropertyName("method")]
    public string Method { get; set; }
}