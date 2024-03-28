using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class Level2Model
{
    [JsonProperty("invoice_id")]
    [JsonPropertyName("invoice_id")]
    public string InvoiceId { get; set; }
    [JsonProperty("tax_total")]
    [JsonPropertyName("tax_total")]
    public AmountModel TaxTotal{ get; set; }
}