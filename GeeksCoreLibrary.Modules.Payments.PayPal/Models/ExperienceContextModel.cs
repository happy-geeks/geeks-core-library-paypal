using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;
///<summary>
///Customizes the payer experience during the approval process for payment with PayPal.
///</summary>
public class ExperienceContextModel
{
        /// <summary>
        /// The label that overrides the business name in the PayPal account on the PayPal site. The pattern is defined by an external party and supports Unicode.
        /// </summary>
        [JsonProperty("brand_name")]
        [JsonPropertyName("brand_name")]
        public string BrandName { get; set; }

        /// <summary>
        /// The location from which the shipping address is derived. Allowed values: GET_FROM_FILE, NO_SHIPPING, SET_PROVIDED_ADDRESS
        /// </summary>
        [JsonProperty("shipping_preference")]
        [JsonPropertyName("shipping_preference")]
        public string ShippingPreference { get; set; }

        /// <summary>
        /// The type of landing page to show on the PayPal site for customer checkout. Allowed values: LOGIN, GUEST_CHECKOUT, NO_PREFERENCE
        ///</summary>
        [JsonProperty("landing_page")]
        [JsonPropertyName("landing_page")]
        public string LandingPage { get; set; }

        /// <summary>
        /// The type of user action that is required. Allowed values: CONTINUE, PAY_NOW
        /// </summary>
        [JsonProperty("user_action")]
        [JsonPropertyName("user_action")]
        public string UserAction { get; set; }

        /// <summary>
        /// The merchant-preferred payment methods. Allowed values: UNRESTRICTED, IMMEDIATE_PAYMENT_REQUIRED
        /// </summary>
        [JsonProperty("payment_method_preference")]
        [JsonPropertyName("payment_method_preference")]
        public string PaymentMethodPreference { get; set; }

        /// <summary>
        /// The BCP 47-formatted locale of pages that the PayPal payment experience shows. PayPal supports a five-character code.
        /// For example, da-DK, he-IL, id-ID, ja-JP, no-NO, pt-BR, ru-RU, sv-SE, th-TH, zh-CN, zh-HK, or zh-TW.
        /// </summary>
        [JsonProperty("locale")]
        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        /// <summary>
        /// The URL where the customer will be redirected upon approving a payment.
        /// </summary>
        [JsonProperty("return_url")]
        [JsonPropertyName("return_url")]
        public string ReturnUrl { get; set; }

        /// <summary>
        /// The URL where the customer will be redirected upon cancelling the payment approval.
        /// </summary>
        [JsonProperty("cancel_url")]
        public string CancelUrl { get; set; }
}