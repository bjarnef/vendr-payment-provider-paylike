using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Paylike
{
    public class PaylikeSettingsBase
    {
        [PaymentProviderSetting(Name = "Continue URL",
            Description = "The URL to continue to after this provider has done processing. eg: /continue/",
            SortOrder = 100)]
        public string ContinueUrl { get; set; }

        [PaymentProviderSetting(Name = "Cancel URL",
            Description = "The URL to return to if the payment attempt is canceled. eg: /cancel/",
            SortOrder = 200)]
        public string CancelUrl { get; set; }

        [PaymentProviderSetting(Name = "Error URL",
            Description = "The URL to return to if the payment attempt errors. eg: /error/",
            SortOrder = 300)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(Name = "Public Key",
            Description = "Public key for Paylike",
            SortOrder = 600)]
        public string PublicKey { get; set; }

        [PaymentProviderSetting(Name = "Locale",
            Description = "Locale for the payment window",
            SortOrder = 700)]
        public string Locale { get; set; }
    }
}
