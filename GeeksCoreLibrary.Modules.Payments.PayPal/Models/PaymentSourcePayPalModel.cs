using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PaymentSourcePayPalModel
{
    /// <summary>
    /// Customizes the payer experience during the approval process for payment with PayPal.
    /// </summary>
    [JsonProperty("experience_context")]
    [JsonPropertyName("experience_context")]
    public ExperienceContextModel ExperienceContext { get; set; }

    /// <summary>
    /// The PayPal billing agreement ID. References an approved recurring payment for goods or services.
    /// </summary>
    [JsonProperty("billing_agreement_id")]
    [JsonPropertyName("billing_agreement_id")]
    public string BillingAgreementId { get; set; }

    /// <summary>
    /// The PayPal-generated ID for the payment_source stored within the Vault.
    /// </summary>
    [JsonProperty("vault_id")]
    [JsonPropertyName("vault_id")]
    public string VaultId { get; set; }

    /// <summary>
    /// The email address of the PayPal account holder.
    /// </summary>
    [JsonProperty("email_address")]
    [JsonPropertyName("email_address")]
    public string EmailAddress { get; set; }

    /// <summary>
    /// The name of the PayPal account holder. Supports only the given_name and surname properties.
    /// </summary>
    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public NameModel Name { get; set; }

    /// <summary>
    /// The phone number of the customer. Available only when you enable the Contact Telephone Number option in the Profile & Settings for the merchant's PayPal account. The phone.phone_number supports only national_number.
    /// </summary>
    [JsonProperty("phone")]
    [JsonPropertyName("phone")]
    public PhoneModel Phone { get; set; }

    /// <summary>
    /// Only used by paypal responses. The ID of the PayPal account (payer ID).
    /// </summary>
    [JsonProperty("account_id")]
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; }

    /// <summary>
    /// Only used by paypal responses. The status of the account (verified or unverified).
    /// </summary>
    [JsonProperty("account_status")]
    [JsonPropertyName("account_status")]
    public string AccountStatus { get; set; }

    /// <summary>
    /// Only used by paypal responses. The address of the PayPal account holder.
    /// </summary>
    [JsonProperty("address")]
    [JsonPropertyName("address")]
    public AddressModel Address { get; set; }

    /// <summary>
    /// The birth date of the PayPal account holder.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore()]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// The birth date of the PayPal account holder in YYYY-MM-DD format.
    /// </summary>
    [JsonProperty("birth_date")]
    [JsonPropertyName("birth_date")]
    public string BirthDateFormatted
    {
        get
        {
            return BirthDate.HasValue ? BirthDate.Value.ToString("yyyy-MM-dd") : null;
        }
    }
}