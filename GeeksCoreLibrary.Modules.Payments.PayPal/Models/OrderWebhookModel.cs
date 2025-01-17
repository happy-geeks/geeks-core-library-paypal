using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class OrderWebhookModel : BaseWebhookModel
{
    [JsonProperty("resource")]
    [JsonPropertyName("resource")]
    public OrderResponseModel Resource { get; set; } = new();
}