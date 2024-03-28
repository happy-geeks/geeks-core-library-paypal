using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class OrderWebhookModel : BaseWebhookModel
{
    [JsonProperty("resource")]
    [JsonPropertyName("resource")]
    public new OrderResponseModel Resource { get; set; }
}