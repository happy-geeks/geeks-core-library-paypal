using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class NameModel
{
    // <summary>
    // When the party is a person, the party's given, or first, name.
    // </summary>
    [JsonProperty("given_name")]
    [JsonPropertyName("given_name")]
    public string GivenName { get; set; }

    // <summary>
    // When the party is a person, the party's surname or family name. Also known as the last name.
    // Required when the party is a person. Use also to store multiple surnames including the matronymic, or mother's, surname.
    // </summary>
    [JsonProperty("surname")]
    [JsonPropertyName("surname")]
    public string Surname { get; set; }
}