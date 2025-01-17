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
public class PayPalService(
    IDatabaseHelpersService databaseHelpersService,
    IDatabaseConnection databaseConnection,
    ILogger<PaymentServiceProviderBaseService> logger,
    IOptions<GclSettings> gclSettings,
    IShoppingBasketsService shoppingBasketsService,
    IWiserItemsService wiserItemsService,
    IObjectsService objectsService,
    IHttpContextAccessor? httpContextAccessor = null)
    : PaymentServiceProviderBaseService(databaseHelpersService, databaseConnection, logger, httpContextAccessor), IPaymentServiceProviderService, IScopedService
{
    private readonly IDatabaseConnection databaseConnection = databaseConnection;
    private readonly ILogger<PaymentServiceProviderBaseService> logger = logger;
    private readonly IHttpContextAccessor? httpContextAccessor = httpContextAccessor;
    private readonly GclSettings gclSettings = gclSettings.Value;

    private string? webHookContents;
    private OrderWebhookModel? webhookData;
    private string? webhookId;
    private string? baseUrl;

    private readonly JsonSerializerSettings? jsonSerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    /// <inheritdoc />
    public async Task<PaymentRequestResult> HandlePaymentRequestAsync(
        ICollection<(WiserItemModel Main, List<WiserItemModel> Lines)> conceptOrders,
        WiserItemModel userDetails,
        PaymentMethodSettingsModel paymentMethodSettings,
        string invoiceNumber)
    {
        var failUrl = "";
        try
        {
            var payPalSettings = (PayPalSettingsModel) paymentMethodSettings.PaymentServiceProvider;
            var validationResult = ValidatePayPalSettings(payPalSettings);
            failUrl = payPalSettings.FailUrl;
            if (!validationResult.Valid)
            {
                logger.LogError("Validation in 'HandlePaymentRequestAsync' of 'PayPalService' failed because: {Message}", validationResult.Message);
                return new PaymentRequestResult
                {
                    Successful = false,
                    Action = PaymentRequestActions.Redirect,
                    ActionData = failUrl
                };
            }

            // Build and execute payment request.
            baseUrl = gclSettings.Environment.InList(Environments.Development, Environments.Test) ? "https://api-m.sandbox.paypal.com/" : "https://api-m.paypal.com/";
            var restClient = CreateRestClient(payPalSettings, baseUrl);
            var restRequest = await CreateRestRequestAsync(payPalSettings, invoiceNumber, conceptOrders);
            restRequest.AddHeader("Content-Type", "application/json");
            var restResponse = await restClient.ExecuteAsync(restRequest);
            if (restResponse.Content == null)
            {
                return new PaymentRequestResult
                {
                    Successful = false,
                    Action = PaymentRequestActions.Redirect,
                    ErrorMessage = "No response received from the PayPal",
                    ActionData = failUrl
                };
            }

            var payPalResponse = JsonConvert.DeserializeObject<OrderResponseModel>(restResponse.Content, jsonSerializerSettings);
            var responseSuccessful = restResponse.StatusCode == HttpStatusCode.OK;
            if (payPalResponse == null)
            {
                return new PaymentRequestResult
                {
                    Successful = false,
                    Action = PaymentRequestActions.Redirect,
                    ErrorMessage = "No response received from PayPal.",
                    ActionData = failUrl
                };
            }

            foreach (var conceptOrder in conceptOrders)
            {
                conceptOrder.Main.SetDetail(OrderProcessConstants.PaymentProviderTransactionId, payPalResponse.Id);
                await wiserItemsService.SaveAsync(conceptOrder.Main, skipPermissionsCheck: true);
            }

            var payerActionLink = payPalResponse.Links.FirstOrDefault(link => String.Equals(link.Rel, "payer-action", StringComparison.OrdinalIgnoreCase));
            return new PaymentRequestResult
            {
                Successful = responseSuccessful,
                Action = PaymentRequestActions.Redirect,
                ErrorMessage = "No response received from PayPal.",
                ActionData = responseSuccessful ? payerActionLink?.Href : payPalSettings.FailUrl
            };
        }
        catch (Exception exception)
        {
            // Log any exceptions that may have occurred.
            logger.LogError(exception, "Error handling PayPal payment request");
            return new PaymentRequestResult
            {
                Successful = false,
                Action = PaymentRequestActions.Redirect,
                ActionData = failUrl
            };
        }
    }

    /// <inheritdoc />
    public async Task<StatusUpdateResult> ProcessStatusUpdateAsync(OrderProcessSettingsModel orderProcessSettings, PaymentMethodSettingsModel paymentMethodSettings)
    {
        var error = "";
        var statusCode = 0;
        try
        {
            if (httpContextAccessor?.HttpContext == null)
            {
                error = "No HTTP context available; unable to process status update.";
                return new StatusUpdateResult
                {
                    Successful = false,
                    Status = error,
                    StatusCode = statusCode
                };
            }

            var paypalVerifyRequestJsonString = $@"{{
		        ""transmission_id"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-TRANSMISSION-ID"].ToString()}"",
		        ""transmission_time"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-TRANSMISSION-TIME"].ToString()}"",
		        ""cert_url"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-CERT-URL"].ToString()}"",
		        ""auth_algo"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-AUTH-ALGO"].ToString()}"",
		        ""transmission_sig"": ""{httpContextAccessor.HttpContext.Request.Headers["PAYPAL-TRANSMISSION-SIG"].ToString()}"",
		        ""webhook_id"": ""{webhookId}"",
		        ""webhook_event"": {webHookContents}
	        }}";

            var payPalSettings = (PayPalSettingsModel) paymentMethodSettings.PaymentServiceProvider;
            if (String.IsNullOrEmpty(baseUrl))
            {
                baseUrl = gclSettings.Environment.InList(Environments.Development, Environments.Test) ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";
            }

            var restClient = CreateRestClient(payPalSettings, baseUrl);
            var restRequest = new RestRequest("/v1/notifications/verify-webhook-signature", Method.Post);
            restRequest.AddParameter("application/json", paypalVerifyRequestJsonString, ParameterType.RequestBody);
            var restResponse = await restClient.ExecuteAsync(restRequest);
            statusCode = (int) restResponse.StatusCode;
            var responseBody = restResponse.Content;

            if (String.IsNullOrEmpty(responseBody))
            {
                error = "No response received from PayPal.";
                return new StatusUpdateResult
                {
                    Successful = false,
                    Status = error,
                    StatusCode = statusCode
                };
            }

            if (restResponse.StatusCode != HttpStatusCode.OK && restResponse.StatusCode != HttpStatusCode.Created && restResponse.StatusCode != HttpStatusCode.NoContent)
            {
                error = "Failed to verify webhook response, because Paypal API returned an error.";
                return new StatusUpdateResult
                {
                    Successful = false,
                    Status = error,
                    StatusCode = statusCode
                };
            }

            var responseJson = JsonConvert.DeserializeObject<VerifyWebhookResponseModel>(responseBody, jsonSerializerSettings);
            if (responseJson?.VerificationStatus != "SUCCESS")
            {
                error = "Invalid webhook.";
                return new StatusUpdateResult
                {
                    Successful = false,
                    Status = error,
                    StatusCode = statusCode
                };
            }

            if (webhookData == null)
            {
                error = "No webhook data available.";
                return new StatusUpdateResult
                {
                    Successful = false,
                    Status = error,
                    StatusCode = statusCode
                };
            }

            if (webhookData.ResourceType != "checkout-order")
            {
                error = "Webhook resource type is not checkout-order. Other types are not supported.";
                return new StatusUpdateResult
                {
                    Successful = false,
                    Status = error,
                    StatusCode = statusCode
                };
            }

            return new StatusUpdateResult
            {
                Successful = responseJson.VerificationStatus.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase),
                Status = responseJson.VerificationStatus
            };
        }
        catch (Exception exception)
        {
            error = exception.ToString();
            // Log any exceptions that may have occurred.
            logger.LogError(exception, "Error processing PayPal payment update.");
            return new StatusUpdateResult
            {
                Successful = false,
                Status = "Error processing PayPal payment update.",
                StatusCode = 500
            };
        }
        finally
        {
            var invoiceNumber = webhookData?.Resource.PurchaseUnits.FirstOrDefault()?.InvoiceId;
            await LogIncomingPaymentActionAsync(PaymentServiceProviders.PayPal, invoiceNumber, statusCode, responseBody: webHookContents, error: error);
        }
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

        try
        {
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

            webhookId = result.WebhookId;
            return result;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error getting provider settings.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string> GetInvoiceNumberFromRequestAsync()
    {
        try
        {
            if (httpContextAccessor?.HttpContext?.Request.Body == null)
            {
                throw new Exception("No HTTP context available.");
            }

            using StreamReader reader = new(httpContextAccessor.HttpContext.Request.Body);
            webHookContents = await reader.ReadToEndAsync();
            if (String.IsNullOrWhiteSpace(webHookContents))
            {
                throw new Exception("No JSON found in body of PayPal webhook.");
            }

            webhookData = JsonConvert.DeserializeObject<OrderWebhookModel>(webHookContents, jsonSerializerSettings);
            if (webhookData == null)
            {
                throw new Exception("Invalid JSON found in body of PayPal webhook.");
            }

            var invoiceId = webhookData.Resource.PurchaseUnits.FirstOrDefault()?.InvoiceId;
            if (String.IsNullOrEmpty(invoiceId))
            {
                throw new Exception("No invoice id found in body of PayPal webhook.");
            }

            return invoiceId;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error getting invoice number from request.");
            throw;
        }
    }

    private static RestClient CreateRestClient(PayPalSettingsModel payPalSettings, string baseUrl)
    {
        return new RestClient(new RestClientOptions(baseUrl)
        {
            Authenticator = new HttpBasicAuthenticator(payPalSettings.ClientId!, payPalSettings.Secret!)
        });
    }

    private (bool Valid, string Message) ValidatePayPalSettings(PayPalSettingsModel payPalSettings)
    {
        if (String.IsNullOrEmpty(payPalSettings.ClientId) || String.IsNullOrEmpty(payPalSettings.Secret))
        {
            return (false, "PayPal misconfigured: No username or password set.");
        }

        return (true, String.Empty);
    }

    private async Task<RestRequest> CreateRestRequestAsync(PayPalSettingsModel payPalSettings, string invoiceNumber, ICollection<(WiserItemModel Main, List<WiserItemModel> Lines)> conceptOrders)
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
                    BirthDate = conceptOrders.FirstOrDefault().Main.GetDetailValue<DateTime>(PayPalConstants.BirthDate) == DateTime.MinValue ? conceptOrders.FirstOrDefault().Main.GetDetailValue<DateTime>(PayPalConstants.BirthDate) : null,
                    ExperienceContext = new ExperienceContextModel()
                }
            },
            PurchaseUnits = []
        };
        payPalCreateOrderRequest.PaymentSource.PayPal.ExperienceContext.BrandName = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_BrandName", searchFromSpecificToGeneral: true);
        payPalCreateOrderRequest.PaymentSource.PayPal.ExperienceContext.Locale = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_Locale", searchFromSpecificToGeneral: true, defaultResult: "nl-NL");
        payPalCreateOrderRequest.PaymentSource.PayPal.ExperienceContext.LandingPage = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_LandingPage", searchFromSpecificToGeneral: true, defaultResult: "NO_PREFERENCE");
        payPalCreateOrderRequest.PaymentSource.PayPal.ExperienceContext.ShippingPreference = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_ShippingPreference", searchFromSpecificToGeneral: true, defaultResult: "SET_PROVIDED_ADDRESS");
        payPalCreateOrderRequest.PaymentSource.PayPal.ExperienceContext.UserAction = await objectsService.FindSystemObjectByDomainNameAsync("PayPal_UserAction", searchFromSpecificToGeneral: true, defaultResult: "PAY_NOW");
        payPalCreateOrderRequest.PaymentSource.PayPal.ExperienceContext.ReturnUrl = payPalSettings.SuccessUrl;
        payPalCreateOrderRequest.PaymentSource.PayPal.ExperienceContext.CancelUrl = payPalSettings.FailUrl;

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
                    CurrencyCode = payPalSettings.Currency,
                    Value = totalPrice,
                    Breakdown = new BreakdownModel
                    {
                        ItemTotal = new AmountModel
                        {
                            CurrencyCode = payPalSettings.Currency,
                            Value = 0
                        },
                        TaxTotal = new AmountModel
                        {
                            CurrencyCode = payPalSettings.Currency,
                            Value = 0
                        },
                        Shipping = new AmountModel
                        {
                            CurrencyCode = payPalSettings.Currency,
                            Value = 0
                        },
                        Handling = new AmountModel
                        {
                            CurrencyCode = payPalSettings.Currency,
                            Value = 0
                        },
                        Discount = new AmountModel
                        {
                            CurrencyCode = payPalSettings.Currency,
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
                        AdminArea2 = conceptOrder.Main.GetDetailValue<string>(hasShippingAddress ? PayPalConstants.ShippingCity : PayPalConstants.City),
                        CountryCode = conceptOrder.Main.GetDetailValue<string>(hasShippingAddress ? PayPalConstants.ShippingCountry : PayPalConstants.Country).ToUpperInvariant()
                    }
                },
                Items = []
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
                        var item = new ItemModel
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
                        };
                        purchaseUnit.Items.Add(item);
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