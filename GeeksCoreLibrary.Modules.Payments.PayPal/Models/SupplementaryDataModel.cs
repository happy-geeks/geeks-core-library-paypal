using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class SupplementaryDataModel
{
    [JsonProperty("card")]
    [JsonPropertyName("card")]
    public CardModel Card { get; set; }
}