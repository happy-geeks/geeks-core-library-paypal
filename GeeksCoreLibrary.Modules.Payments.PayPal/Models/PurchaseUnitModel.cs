using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PurchaseUnitModel
{
    [JsonProperty("reference_id")]
    [JsonPropertyName("reference_id")]
    public string ReferenceId { get; set; }
    [JsonProperty("description")]
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonProperty("custom_id")]
    [JsonPropertyName("custom_id")]
    public string CustomId { get; set; }
    [JsonProperty("soft_descriptor")]
    [JsonPropertyName("soft_descriptor")]
    public string SoftDescriptor { get; set; }
    [JsonProperty("invoice_id")]
    [JsonPropertyName("invoice_id")]
    public string InvoiceId { get; set; }
    [JsonProperty("supplementary_data")]
    [JsonPropertyName("supplementary_data")]
    public SupplementaryDataModel SupplementaryData { get; set; }
    [JsonProperty("amount")]
    [JsonPropertyName("amount")]
    public AmountModel Amount { get; set; }
    [JsonProperty("items")]
    [JsonPropertyName("items")]
    public List<ItemModel> Items { get; set; }
    [JsonProperty("shipping")]
    [JsonPropertyName("shipping")]
    public ShippingModel Shipping { get; set; }
    [JsonProperty("payee")]
    [JsonPropertyName("payee")]
    public PayerModel Payee { get; set; }
    [JsonProperty("payments")]
    [JsonPropertyName("payments")]
    public PaymentsModel Payments { get; set; }
}