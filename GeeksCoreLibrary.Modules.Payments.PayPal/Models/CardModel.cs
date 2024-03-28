using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class CardModel
{
    [JsonProperty("level_2")]
    [JsonPropertyName("level_2")]
    public Level2Model Level2 { get; set; }
    [JsonProperty("level_3")]
    [JsonPropertyName("level_3")]
    public Level3Model Level3 { get; set; }
}