using System.Net;
using GeeksCoreLibrary.Components.OrderProcess.Models;
using GeeksCoreLibrary.Components.ShoppingBasket;
using GeeksCoreLibrary.Components.ShoppingBasket.Interfaces;
using GeeksCoreLibrary.Core.DependencyInjection.Interfaces;
using GeeksCoreLibrary.Core.Enums;
using GeeksCoreLibrary.Core.Extensions;
using GeeksCoreLibrary.Core.Interfaces;
using GeeksCoreLibrary.Core.Models;
using GeeksCoreLibrary.Modules.Databases.Interfaces;
using GeeksCoreLibrary.Modules.Objects.Interfaces;
using GeeksCoreLibrary.Modules.Payments.Enums;
using GeeksCoreLibrary.Modules.Payments.Interfaces;
using GeeksCoreLibrary.Modules.Payments.Models;
using GeeksCoreLibrary.Modules.Payments.OneBip.Models;
using GeeksCoreLibrary.Modules.Payments.PayPal.Models;
using GeeksCoreLibrary.Modules.Payments.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using OrderProcessConstants = GeeksCoreLibrary.Components.OrderProcess.Models.Constants;

namespace GeeksCoreLibrary.Modules.Payments.PayPal.Services;

/// <inheritdoc cref="IPaymentServiceProviderService" />
public class PayPalService : PaymentServiceProviderBaseService, IPaymentServiceProviderService, IScopedService
{
    private const string BaseUrl = "https://api-m.sandbox.paypal.com/";
    private readonly IDatabaseConnection databaseConnection;
    private readonly ILogger<PaymentServiceProviderBaseService> logger;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly GclSettings gclSettings;
    private readonly IShoppingBasketsService shoppingBasketsService;
    private readonly IWiserItemsService wiserItemsService;
    private readonly IObjectsService objectsService;

    private string WebHookContents = null;
    private OrderWebhookModel WebhookData = null;
    private string WebhookId = null;

    private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    public PayPalService(
        IDatabaseHelpersService databaseHelpersService,
        IDatabaseConnection databaseConnection,
        ILogger<PaymentServiceProviderBaseService> logger,
        IOptions<GclSettings> gclSettings,
        IShoppingBasketsService shoppingBasketsService,
        IWiserItemsService wiserItemsService,
        IObjectsService objectsService,
        IHttpContextAccessor httpContextAccessor = null) : base(databaseHelpersService, databaseConnection, logger, httpContextAccessor)
    {
        this.databaseConnection = databaseConnection;
        this.logger = logger;
        this.shoppingBasketsService = shoppingBasketsService;
        this.wiserItemsService = wiserItemsService;
        this.objectsService = objectsService;
        this.gclSettings = gclSettings.Value;
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public async Task<PaymentRequestResult> HandlePaymentRequestAsync(ICollection<(WiserItemModel Main, List<WiserItemModel> Lines)> conceptOrders, WiserItemModel userDetails,
        PaymentMethodSettingsModel paymentMethodSettings, string invoiceNumber)
    {
        var payPalSettings = (PayPalSettingsModel)paymentMethodSettings.PaymentServiceProvider;
        var validationResult = ValidatePayPalSettings(payPalSettings);
        if (!validationResult.Valid)
        {
            logger.LogError("Validation in 'HandlePaymentRequestAsync' of 'PayPalService' failed because: {Message}", validationResult.Message);
            return new PaymentRequestResult
            {
                Successful = false,
                Action = PaymentRequestActions.Redirect,
                ActionData = payPalSettings.FailUrl
            };
        }
        
        // Build and execute payment request.
        var restClient = CreateRestClient(payPalSettings);
        var restRequest = await CreateRestRequestAsync(payPalSettings, invoiceNumber, conceptOrders);
        restRequest.AddHeader("Content-Type", "application/json");
        var restResponse = await restClient.ExecuteAsync(restRequest);
        var payPalResponse = JsonConvert.DeserializeObject<OrderResponseModel>(restResponse.Content, jsonSerializerSettings);
        var responseSuccessful = restResponse.StatusCode == HttpStatusCode.OK;

        foreach (var conceptOrder in conceptOrders)
        {
            conceptOrder.Main.SetDetail(OrderProcessConstants.PaymentProviderTransactionId, payPalResponse.Id);
            await wiserItemsService.SaveAsync(conceptOrder.Main, skipPermissionsCheck: true);
        }
        
        var payerActionLink = payPalResponse.Links.FirstOrDefault(link => string.Equals(link.Rel, "payer-action", StringComparison.OrdinalIgnoreCase));
        return new PaymentRequestResult
        {
            Successful = responseSuccessful,
            Action = PaymentRequestActions.Redirect,
            ActionData = (responseSuccessful) ? payerActionLink.Href :  payPalSettings.FailUrl 
        };
    }

    /// <inheritdoc />
    public async Task<StatusUpdateResult> ProcessStatusUpdateAsync(OrderProcessSettingsModel orderProcessSettings, PaymentMethodSettingsModel paymentMethodSettings)
    {
        if (httpContextAccessor?.HttpContext == null)
        {
            return new StatusUpdateResult
            {
                Successful = false,
                Status = "Error retrieving status: No HttpContext available."
            };
        }
        
        var paypalVerifyRequestJsonString = $@"{{
				    ""transmission_id"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-TRANSMISSION-ID"].ToString()}"",
				    ""transmission_time"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-TRANSMISSION-TIME"].ToString()}"",
				    ""cert_url"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-CERT-URL"].ToString()}"",
				    ""auth_algo"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-AUTH-ALGO"].ToString()}"",
				    ""transmission_sig"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-TRANSMISSION-SIG"].ToString()}"",
				    ""webhook_id"": ""{WebhookId}"",
				    ""webhook_event"": {WebHookContents}
				}}";
        
        var payPalSettings = (PayPalSettingsModel)paymentMethodSettings.PaymentServiceProvider;
        var restClient = CreateRestClient(payPalSettings);
        var restRequest = new RestRequest("/v1/notifications/verify-webhook-signature", Method.Post);
        restRequest.AddParameter("application/json", paypalVerifyRequestJsonString, ParameterType.RequestBody);
        var restResponse = await restClient.ExecuteAsync(restRequest);
        
        if (restResponse.StatusCode != HttpStatusCode.OK && restResponse.StatusCode != HttpStatusCode.Created && restResponse.StatusCode != HttpStatusCode.NoContent)
        {
            throw new Exception("Failed to verify webhook response, because Paypal API returned an error.");
        }
        
        var responseJson = JsonConvert.DeserializeObject<VerifyWebhookResponseModel>(restResponse.Content, jsonSerializerSettings);
        if (responseJson?.VerificationStatus != "SUCCESS")
        {
            throw new Exception("Invalid webhook");
        }

        if (WebhookData.ResourceType != "checkout-order")
        {
            throw new Exception("Webhook resource type is not checkout-order. Other types are not supported.");
        }
        
        var invoiceNumber = WebhookData.Resource.PurchaseUnits.FirstOrDefault()?.InvoiceId;
        await LogIncomingPaymentActionAsync(PaymentServiceProviders.PayPal, invoiceNumber, (int)restResponse.StatusCode, responseBody: restResponse.Content);
        return new StatusUpdateResult
        {
            Successful = responseJson.VerificationStatus.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase),
            Status = responseJson.VerificationStatus
        };
    }

    /// <inheritdoc />
    public async Task<PaymentServiceProviderSettingsModel> GetProviderSettingsAsync(PaymentServiceProviderSettingsModel paymentServiceProviderSettings)
    {
        databaseConnection.AddParameter("id", paymentServiceProviderSettings.Id);
        var query = $@"SELECT
        payPalClientIdLive.`value` AS payPalClientIdLive,
        payPalClientIdTest.`value` AS payPalClientIdTest,
        payPalSecretLive.`value` AS payPalSecretLive,
        payPalSecretTest.`value` AS payPalSecretTest,
        webhookIdLive.`value` AS webhookIdLive,
        webhookIdTest.`value` AS webhookIdTest
        FROM {WiserTableNames.WiserItem} AS paymentServiceProvider
        LEFT JOIN {WiserTableNames.WiserItemDetail} AS payPalClientIdLive ON payPalClientIdLive.item_id = paymentServiceProvider.id AND payPalClientIdLive.`key` = '{PayPalConstants.PayPalClientIdLive}'
        LEFT JOIN {WiserTableNames.WiserItemDetail} AS payPalClientIdTest ON payPalClientIdTest.item_id = paymentServiceProvider.id AND payPalClientIdTest.`key` = '{PayPalConstants.PayPalClientIdTest}'
        LEFT JOIN {WiserTableNames.WiserItemDetail} AS payPalSecretLive ON payPalSecretLive.item_id = paymentServiceProvider.id AND payPalSecretLive.`key` = '{PayPalConstants.PayPalSecretLive}'
        LEFT JOIN {WiserTableNames.WiserItemDetail} AS payPalSecretTest ON payPalSecretTest.item_id = paymentServiceProvider.id AND payPalSecretTest.`key` = '{PayPalConstants.PayPalSecretTest}'
        LEFT JOIN {WiserTableNames.WiserItemDetail} AS webhookIdLive ON webhookIdLive.item_id = paymentServiceProvider.id AND webhookIdLive.`key` = '{PayPalConstants.WebhookIdLive}'
        LEFT JOIN {WiserTableNames.WiserItemDetail} AS webhookIdTest ON webhookIdTest.item_id = paymentServiceProvider.id AND webhookIdTest.`key` = '{PayPalConstants.WebhookIdTest}'
        WHERE paymentServiceProvider.id = ?id";

        var result = new PayPalSettingsModel
        {
            Id = paymentServiceProviderSettings.Id,
            Title = paymentServiceProviderSettings.Title,
            Type = paymentServiceProviderSettings.Type,
            LogAllRequests = paymentServiceProviderSettings.LogAllRequests,
            OrdersCanBeSetDirectlyToFinished = paymentServiceProviderSettings.OrdersCanBeSetDirectlyToFinished,
            SkipPaymentWhenOrderAmountEqualsZero = paymentServiceProviderSettings.SkipPaymentWhenOrderAmountEqualsZero
        };
        var dataTable = await databaseConnection.GetAsync(query);
        if (dataTable.Rows.Count == 0)
        {
            return result;
        }
        var row = dataTable.Rows[0];

        var suffix = gclSettings.Environment.InList(Environments.Development, Environments.Test) ? "Test" : "Live";
        result.ClientId = row.GetAndDecryptSecretKey($"payPalClientId{suffix}");
        result.Secret = row.GetAndDecryptSecretKey($"payPalSecret{suffix}");
        result.WebhookId = row.GetAndDecryptSecretKey($"webhookId{suffix}");
        if (String.IsNullOrWhiteSpace(result.WebhookId))
        {
            throw new Exception("No PayPal webhook id found.");
        }
        WebhookId = result.WebhookId;
        return result;
    }

    /// <inheritdoc />
    public string GetInvoiceNumberFromRequest()
    {
        using StreamReader reader = new(httpContextAccessor.HttpContext.Request.Body);
        WebHookContents = reader.ReadToEndAsync().Result;
        if (String.IsNullOrWhiteSpace(WebHookContents))
        {
            throw new Exception("No JSON found in body of PayPal webhook.");
        }

        WebhookData = JsonConvert.DeserializeObject<OrderWebhookModel>(WebHookContents, jsonSerializerSettings);
        if (WebhookData == null)
        {
            throw new Exception("Invalid JSON found in body of PayPal webhook.");
        }

        return WebhookData.Resource.PurchaseUnits.FirstOrDefault().InvoiceId;
    }
    private static RestClient CreateRestClient(PayPalSettingsModel payPalSettings)
    {
        return new RestClient(new RestClientOptions(BaseUrl)
        {
            Authenticator = new HttpBasicAuthenticator(payPalSettings.ClientId, payPalSettings.Secret),
        });
    }
    
    private (bool Valid, string Message) ValidatePayPalSettings(PayPalSettingsModel payPalSettings)
    {
        if (String.IsNullOrEmpty(payPalSettings.ClientId) || String.IsNullOrEmpty(payPalSettings.Secret))
        {
            return (false, "PayPal misconfigured: No username or password set.");
        }

        return (true, null);
    }
    
    private async Task<RestRequest> CreateRestRequestAsync( PayPalSettingsModel payPalSettings, string invoiceNumber, ICollection<(WiserItemModel Main, List<WiserItemModel> Lines)> conceptOrders)
    {
        var restRequest = new RestRequest("/v2/checkout/orders", Method.Post);
        var payPalCreateOrderRequest = new OrderRequestModel
        {
            Intent = "CAPTURE",
            PaymentSource = new PaymentSourceModel
            {
                PayPal = new PaymentSourcePayPalModel
                {
                    EmailAddress = conceptOrders.FirstOrDefault().Main.GetDetailValue<string>(PayPalConstants.EmailAddress),
                    Name = new NameModel
                    {
                        GivenName = conceptOrders.FirstOrDefault().Main.GetDetailValue<string>(PayPalConstants.GivenName),
                        Surname = conceptOrders.FirstOrDefault().Main.GetDetailValue<string>(PayPalConstants.Surname)
                    },
                    BirthDate = conceptOrders.FirstOrDefault().Main.GetDetailValue<DateTime>(PayPalConstants.BirthDate) == DateTime.MinValue
                            ? conceptOrders.FirstOrDefault().Main.GetDetailValue<DateTime>(PayPalConstants.BirthDate)
                            : null,
                    ExperienceContext = new ExperienceContextModel
                    {
                        BrandName = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_BrandName", searchFromSpecificToGeneral: true, defaultResult: "voordeelgordijnen"),
                        Locale = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_Locale", searchFromSpecificToGeneral: true, defaultResult: "nl-NL"),
                        LandingPage = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_LandingPage", searchFromSpecificToGeneral: true, defaultResult: "NO_PREFERENCE"),
                        ShippingPreference = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_ShippingPreference", searchFromSpecificToGeneral: true, defaultResult: "SET_PROVIDED_ADDRESS"),
                        UserAction = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_UserAction", searchFromSpecificToGeneral: true, defaultResult: "PAY_NOW"),
                        ReturnUrl = payPalSettings.SuccessUrl,
                        CancelUrl = payPalSettings.FailUrl
                    }
                }
            },
            PurchaseUnits = new List<PurchaseUnitModel>()
        };
        
        var basketSettings = await shoppingBasketsService.GetSettingsAsync();
        
        foreach (var conceptOrder in conceptOrders)
        {
            var hasShippingAddress = !String.IsNullOrWhiteSpace(conceptOrder.Main.GetDetailValue<string>(PayPalConstants.ShippingPostalCode));
            
            var totalPrice = await shoppingBasketsService.GetPriceAsync(conceptOrder.Main, conceptOrder.Lines, basketSettings, ShoppingBasket.PriceTypes.PspPriceInVat);
            var purchaseUnit = new PurchaseUnitModel
            {
                ReferenceId = conceptOrder.Main.Id.ToString(),
                InvoiceId = invoiceNumber,
                Amount = new AmountModel
                {
                    CurrencyCode =  payPalSettings.Currency,
                    Value = totalPrice,
                    Breakdown = new BreakdownModel
                    {
                        ItemTotal = new AmountModel
                        {
                            CurrencyCode =  payPalSettings.Currency,
                            Value = 0
                        },
                        TaxTotal = new AmountModel
                        {
                            CurrencyCode =  payPalSettings.Currency,
                            Value = 0
                        },
                        Shipping = new AmountModel
                        {
                            CurrencyCode =  payPalSettings.Currency,
                            Value = 0
                        },
                        Handling = new AmountModel
                        {
                            CurrencyCode =  payPalSettings.Currency,
                            Value = 0
                        },
                        Discount = new AmountModel
                        {
                            CurrencyCode =  payPalSettings.Currency,
                            Value = 0
                        }
                    }
                },
                Shipping = new ShippingModel
                {
                    Address = new AddressModel
                    {
                        AddressLine1 = $"{conceptOrder.Main.GetDetailValue<string>(hasShippingAddress ? PayPalConstants.ShippingStreet : PayPalConstants.Street)} {conceptOrder.Main.GetDetailValue<string>(hasShippingAddress ? PayPalConstants.ShippingHouseNumber : PayPalConstants.HouseNumber)}",
                        AddressLine2 = conceptOrder.Main.GetDetailValue<string>(hasShippingAddress ? PayPalConstants.ShippingHouseNumberSuffix : PayPalConstants.HouseNumberSuffix),
                        PostalCode = conceptOrder.Main.GetDetailValue<string>(hasShippingAddress ? PayPalConstants.ShippingPostalCode : PayPalConstants.PostalCode),
                        AdminArea2 = conceptOrder.Main.GetDetailValue<string>(hasShippingAddress ? PayPalConstants.ShippingCity :PayPalConstants.City),
                        CountryCode = conceptOrder.Main.GetDetailValue<string>(hasShippingAddress ? PayPalConstants.ShippingCountry : PayPalConstants.Country).ToUpperInvariant()
                    }
                },
                Items = new List<ItemModel>()
            };
            
            payPalCreateOrderRequest.PurchaseUnits.Add(purchaseUnit);
            foreach (var orderLine in conceptOrder.Lines)
            {
                var sku = orderLine.GetDetailValue(PayPalConstants.Sku);
                var lineType = orderLine.GetDetailValue("type");
                var linePriceExcludingTaxes = await shoppingBasketsService.GetLinePriceAsync(conceptOrder.Main, orderLine, basketSettings, ShoppingBasket.PriceTypes.ExVatExDiscount, true);
                var linePriceIncludingTaxes = await shoppingBasketsService.GetLinePriceAsync(conceptOrder.Main, orderLine, basketSettings, ShoppingBasket.PriceTypes.InVatExDiscount, true);
                
                switch (lineType.ToUpperInvariant())
                {
                    case "COUPON":
                        purchaseUnit.Amount.Breakdown.Discount.Value += linePriceIncludingTaxes;
                        break;
                    case "SHIPPING_COSTS":
                        purchaseUnit.Amount.Breakdown.Shipping.Value += linePriceIncludingTaxes;
                        break;
                    case "PAYMENTMETHOD_COSTS":
                        purchaseUnit.Amount.Breakdown.Handling.Value += linePriceIncludingTaxes;
                        break;
                    case "PRODUCT":
                    {
                        var quantity = orderLine.GetDetailValue<int>(PayPalConstants.Quantity);
                        var taxes = await shoppingBasketsService.GetLinePriceAsync(conceptOrder.Main, orderLine, basketSettings, ShoppingBasket.PriceTypes.VatOnly, true);
                        purchaseUnit.Amount.Breakdown.ItemTotal.Value += Math.Round(linePriceExcludingTaxes, 2) * quantity;
                        purchaseUnit.Amount.Breakdown.TaxTotal.Value += Math.Round(taxes, 2) * quantity;
                        purchaseUnit.Items.Add(new ItemModel
                        {
                            Name = orderLine.GetDetailValue<string>(PayPalConstants.Title),
                            Sku = sku,
                            Quantity = quantity,

                            Category = PayPalConstants.Category,
                            UnitAmount = new AmountModel
                            {
                                CurrencyCode = payPalSettings.Currency,
                                Value = Math.Round(linePriceExcludingTaxes, 2)
                            },
                            Tax = new AmountModel
                            {
                                CurrencyCode = payPalSettings.Currency,
                                Value = Math.Round(taxes, 2)
                            }
                        });

                        break;
                    }
                    default:
                        purchaseUnit.Amount.Breakdown.Handling.Value += linePriceIncludingTaxes;
                        break;
                }
            }

            // Round down everything at the end, after the total prices have been calculated.
            purchaseUnit.Amount.Breakdown.Shipping.Value = Math.Round(purchaseUnit.Amount.Breakdown.Shipping.Value, 2);
            purchaseUnit.Amount.Breakdown.Handling.Value = Math.Round(purchaseUnit.Amount.Breakdown.Handling.Value, 2);
            purchaseUnit.Amount.Breakdown.Discount.Value = Math.Round(purchaseUnit.Amount.Breakdown.Discount.Value, 2);

            // Calculate if there is a rounding difference and add it to the handling costs.
            var total = purchaseUnit.Amount.Breakdown.ItemTotal.Value +
                        purchaseUnit.Amount.Breakdown.TaxTotal.Value +
                        purchaseUnit.Amount.Breakdown.Shipping.Value +
                        purchaseUnit.Amount.Breakdown.Handling.Value +
                        purchaseUnit.Amount.Breakdown.Discount.Value;

            var roundingDifference = totalPrice - total;
            purchaseUnit.Amount.Breakdown.Handling.Value += Math.Round(roundingDifference, 2);
        }
        restRequest.AddJsonBody(payPalCreateOrderRequest);

        return restRequest;
    }
}