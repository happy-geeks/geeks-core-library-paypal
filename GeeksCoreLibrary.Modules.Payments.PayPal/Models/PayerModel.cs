using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PayerModel
{
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public NameModel Name { get; set; }
    [JsonProperty("email_address")]
    [JsonPropertyName("email_address")]
    public string EmailAddress { get; set; }
    [JsonProperty("payer_id")]
    [JsonPropertyName("payer_id")]
    public string PayerId { get; set; }
}