using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PaymentWebhookModel : BaseWebhookModel
{
    [JsonProperty("resource")]
    [JsonPropertyName("resource")]
    public new PaymentModel Resource { get; set; }
}