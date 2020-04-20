﻿using Vendr.Contrib.PaymentProviders.Paylike.Api.Models;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Paylike
{
    public abstract class PaylikePaymentProviderBase<TSettings> : PaymentProviderBase<TSettings>
            where TSettings : PaylikeSettingsBase, new()
    {
        public PaylikePaymentProviderBase(VendrContext vendr)
            : base(vendr)
        { }

        public override string GetCancelUrl(OrderReadOnly order, TSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.CancelUrl.MustNotBeNull("settings.CancelUrl");

            return settings.CancelUrl;
        }

        public override string GetContinueUrl(OrderReadOnly order, TSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return settings.ContinueUrl;
        }

        public override string GetErrorUrl(OrderReadOnly order, TSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ErrorUrl.MustNotBeNull("settings.ErrorUrl");

            return settings.ErrorUrl;
        }

        protected PaylikeClientConfig GetPaylikeClientConfig(PaylikeSettingsBase settings)
        {
            return new PaylikeClientConfig
            {
                BaseUrl = "https://api.paylike.io"
            };
        }
    }
}
