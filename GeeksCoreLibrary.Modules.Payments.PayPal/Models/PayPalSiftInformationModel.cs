namespace GeeksCoreLibrary.Modules.Payments.PayPal.Models;

public class PayPalSiftInformationModel
{
    public string PayerId { get; set; }
    public string PayerEmail { get; set; }
    public string PayerStatus { get; set; }
    public string AddressStatus { get; set; }
    public string ProtectionEligibility { get; set; }
    public string PaymentStatus { get; set; }
}