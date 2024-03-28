using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PaymentsModel
{
    [JsonProperty("captures")]
    [JsonPropertyName("captures")]
    public List<CaptureModel> Captures { get; set; }
}