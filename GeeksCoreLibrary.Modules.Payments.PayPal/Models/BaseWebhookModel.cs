using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class BaseWebhookModel
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonProperty("create_time")]
    [JsonPropertyName("create_time")]
    public DateTimeOffset CreateTime { get; set; }
    [JsonProperty("resource_type")]
    [JsonPropertyName("resource_type")]
    public string ResourceType { get; set; }
    [JsonProperty("event_type")]
    [JsonPropertyName("event_type")]
    public string EventType { get; set; }
    [JsonProperty("summary")]
    [JsonPropertyName("summary")]
    public string Summary { get; set; }
    [JsonProperty("links")]
    [JsonPropertyName("links")]
    public List<LinkModel> Links { get; set; }
    [JsonProperty("event_version")]
    [JsonPropertyName("event_version")]
    public string EventVersion { get; set; }
}