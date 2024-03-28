using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PhoneModel
{
    /// <summary>
    /// The phone type. Supported values: HOME, PAGER, MOBILE, FAX, OTHER.
    /// </summary>
    [JsonProperty("phone_type")]
    [JsonPropertyName("phone_type")]
    public string PhoneType { get; set; }

    /// <summary>
    /// The phone number, in its canonical international E.164 numbering plan format. Supports only the national_number property.
    /// </summary>
    [JsonProperty("phone_number")]
    [JsonPropertyName("phone_number")]
    public PhoneNumberModel PhoneNumber { get; set; }
}