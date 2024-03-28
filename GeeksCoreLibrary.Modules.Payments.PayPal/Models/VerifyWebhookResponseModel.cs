using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class VerifyWebhookResponseModel
{
    [JsonProperty("verification_status")]
    [JsonPropertyName("verification_status")]
    public string VerificationStatus { get; set; }
}