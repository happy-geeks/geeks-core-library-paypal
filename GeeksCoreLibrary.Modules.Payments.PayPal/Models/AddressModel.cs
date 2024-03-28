using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;


public class AddressModel
{
    [JsonProperty("address_line_1")]
    [JsonPropertyName("address_line_1")]
    public string AddressLine1 { get; set; }
    [JsonProperty("address_line_2")]
    [JsonPropertyName("address_line_2")]
    public string AddressLine2 { get; set; }
    [JsonProperty("admin_area_2")]
    [JsonPropertyName("admin_area_2")]
    public string AdminArea2 { get; set; }
    [JsonProperty("admin_area_1")]
    [JsonPropertyName("admin_area_1")]
    public string AdminArea1 { get; set; }
    [JsonProperty("postal_code")]
    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }
    [JsonProperty("country_code")]
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }
}
