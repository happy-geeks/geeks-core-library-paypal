using GeeksCoreLibrary.Components.OrderProcess.Models;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PayPalSettingsModel : PaymentServiceProviderSettingsModel
{

    /// <summary>
    /// Gets or sets the client id.
    /// This is 
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the secret.
    /// This is a token 
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    /// Gets or sets the Service ID. Required if logging in with an AT-code/token
    /// </summary>
    public string WebhookId { get; set; }

    
}