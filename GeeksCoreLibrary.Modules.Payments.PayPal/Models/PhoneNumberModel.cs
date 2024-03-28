using System.Text.Json.Serialization;
using Newtonsoft.Json; 

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PhoneNumberModel
{
    /// <summary>
    /// The national number, in its canonical international E.164 numbering plan format.
    /// The combined length of the country calling code (CC) and the national number must not be greater than 15 digits.
    /// The national number consists of a national destination code (NDC) and subscriber number (SN).
    /// </summary>
    [JsonProperty("national_number")]
    [JsonPropertyName("national_number")]
    public string NationalNumber { get; set; }
}