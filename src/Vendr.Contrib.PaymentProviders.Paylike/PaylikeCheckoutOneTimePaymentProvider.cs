﻿using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Vendr.Contrib.PaymentProviders.Paylike.Api;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Paylike
{
    [PaymentProvider("paylike-checkout-onetime", "Paylike (One Time)", "Paylike payment provider for one time payments")]
    public class PaylikeCheckoutOneTimePaymentProvider : PaylikePaymentProviderBase<PaylikeCheckoutOneTimeSettings>
    {
        public PaylikeCheckoutOneTimePaymentProvider(VendrContext vendr)
            : base(vendr)
        { }

        public override bool FinalizeAtContinueUrl => true;

        public override PaymentFormResult GenerateForm(OrderReadOnly order, string continueUrl, string cancelUrl, string callbackUrl, PaylikeCheckoutOneTimeSettings settings)
        {
            var currency = Vendr.Services.CurrencyService.GetCurrency(order.CurrencyId);
            var currencyCode = currency.Code.ToUpperInvariant();

            // Ensure currency has valid ISO 4217 code
            if (!Iso4217.CurrencyCodes.ContainsKey(currencyCode))
            {
                throw new Exception("Currency must be a valid ISO 4217 currency code: " + currency.Name);
            }

            var orderAmount = AmountToMinorUnits(order.TotalPrice.Value.WithTax);

            string paymentFormLink = string.Empty;

            try
            {
                //var paymentLink = $"https://pos.paylike.io?key={settings.PublicKey}" +
                //                    $"&currency={currencyCode}" +
                //                    $"&amount={orderAmount}" +
                //                    $"&reference={order.OrderNumber}" +
                //                    $"&text=" +
                //                    $"&redirect={continueUrl}" +
                //                    $"&locale={settings.Locale}";

                //paymentFormLink = paymentLink;
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<PaylikeCheckoutOneTimePaymentProvider>(ex, "Paylike - error creating payment.");
            }

            return new PaymentFormResult()
            {
                Form = new PaymentForm(continueUrl, FormMethod.Post)
                        .WithAttribute("onsubmit", "return handlePaylikeCheckout(event)")
                        .WithJsFile("https://sdk.paylike.io/3.js")
                        .WithJs(@"
                            var paylike = Paylike('" + settings.PublicKey + @"');
                            
                            window.handlePaylikeCheckout = function (e) {
                                e.preventDefault();

                                var config = 
                                {
		                            currency: '" + currencyCode + @"',
		                            amount: " + orderAmount + @",
                                    locale: '" + settings.Locale + @"',
		                            custom: {
			                            orderId: '" + order.OrderNumber + @"'
		                            },
	                            };

                                paylike.popup(config, function(err, res) {
		                            if (err)
			                            return console.log(err);

		                            location.href = '" + continueUrl + @"';
	                            });
                                
                                return false;
                            }
                        ")
            };
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, PaylikeCheckoutOneTimeSettings settings)
        {
            try
            {
                var clientConfig = GetPaylikeClientConfig(settings);
                var client = new PaylikeClient(clientConfig);

                //var payment = client.FetchTransaction(order.TransactionInfo.TransactionId);

                return new CallbackResult
                {
                    TransactionInfo = new TransactionInfo
                    {
                        AmountAuthorized = order.TotalPrice.Value.WithTax,
                        TransactionFee = 0m,
                        TransactionId = Guid.NewGuid().ToString("N"),
                        PaymentStatus = PaymentStatus.Authorized
                    }
                };
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<PaylikeCheckoutOneTimePaymentProvider>(ex, "Paylike - ProcessCallback");
            }

            return CallbackResult.Empty;
        }

        public override ApiResult FetchPaymentStatus(OrderReadOnly order, PaylikeCheckoutOneTimeSettings settings)
        {
            // Get payment: https://github.com/paylike/api-docs#fetch-a-transaction

            try
            {
                var clientConfig = GetPaylikeClientConfig(settings);
                var client = new PaylikeClient(clientConfig);

                var payment = client.FetchTransaction(order.TransactionInfo.TransactionId);

                //return new ApiResult()
                //{
                //    TransactionInfo = new TransactionInfoUpdate()
                //    {
                //        TransactionId = GetTransactionId(payment),
                //        PaymentStatus = GetPaymentStatus(payment)
                //    }
                //};
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<PaylikeCheckoutOneTimePaymentProvider>(ex, "Paylike - FetchPaymentStatus");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CancelPayment(OrderReadOnly order, PaylikeCheckoutOneTimeSettings settings)
        {
            // Cancel payment: https://github.com/paylike/api-docs#void-a-transaction

            try
            {
                var clientConfig = GetPaylikeClientConfig(settings);
                var client = new PaylikeClient(clientConfig);

                var data = new
                {
                    amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                var payment = client.VoidTransaction(order.TransactionInfo.TransactionId, data);

                //return new ApiResult()
                //{
                //    TransactionInfo = new TransactionInfoUpdate()
                //    {
                //        TransactionId = GetTransactionId(payment),
                //        PaymentStatus = GetPaymentStatus(payment)
                //    }
                //};
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<PaylikeCheckoutOneTimePaymentProvider>(ex, "Paylike - CancelPayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CapturePayment(OrderReadOnly order, PaylikeCheckoutOneTimeSettings settings)
        {
            // Capture payment: https://github.com/paylike/api-docs#capture-a-transaction

            try
            {
                var clientConfig = GetPaylikeClientConfig(settings);
                var client = new PaylikeClient(clientConfig);

                var data = new
                {
                    amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                var payment = client.CaptureTransaction(order.TransactionInfo.TransactionId, data);

                //return new ApiResult()
                //{
                //    TransactionInfo = new TransactionInfoUpdate()
                //    {
                //        TransactionId = GetTransactionId(payment),
                //        PaymentStatus = GetPaymentStatus(payment)
                //    }
                //};
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<PaylikeCheckoutOneTimeSettings>(ex, "Paylike - CapturePayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult RefundPayment(OrderReadOnly order, PaylikeCheckoutOneTimeSettings settings)
        {
            // Refund payment: https://github.com/paylike/api-docs#refund-a-transaction

            try
            {
                var clientConfig = GetPaylikeClientConfig(settings);
                var client = new PaylikeClient(clientConfig);

                var data = new
                {
                    amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                var payment = client.RefundTransaction(order.TransactionInfo.TransactionId, data);

                //return new ApiResult()
                //{
                //    TransactionInfo = new TransactionInfoUpdate()
                //    {
                //        TransactionId = GetTransactionId(refund),
                //        PaymentStatus = GetPaymentStatus(refund)
                //    }
                //};
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<PaylikeCheckoutOneTimePaymentProvider>(ex, "Paylike - RefundPayment");
            }

            return ApiResult.Empty;
        }
    }
}
