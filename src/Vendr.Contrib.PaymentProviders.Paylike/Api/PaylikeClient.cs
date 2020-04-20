using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Vendr.Contrib.PaymentProviders.Paylike.Api.Models;

namespace Vendr.Contrib.PaymentProviders.Paylike.Api
{
    public class PaylikeClient
    {
        private PaylikeClientConfig _config;

        public PaylikeClient(PaylikeClientConfig config)
        {
            _config = config;
        }

        public string FetchTransaction(string transactionId)
        {
            return Request($"/transactions/{transactionId}", (req) => req
                .GetJsonAsync<string>());
        }

        public string VoidTransaction(string transactionId, object data)
        {
            return Request($"/transactions/{transactionId}/voids", (req) => req
                .PostJsonAsync(data)
                .ReceiveJson<string>());
        }

        public string CaptureTransaction(string transactionId, object data)
        {
            return Request($"/transactions/{transactionId}/captures", (req) => req
                .PostJsonAsync(data)
                .ReceiveJson<string>());
        }

        public string RefundTransaction(string transactionId, object data)
        {
            return Request($"/transactions/{transactionId}/refunds", (req) => req
                .PostJsonAsync(data)
                .ReceiveJson<string>());
        }

        private TResult Request<TResult>(string url, Func<IFlurlRequest, Task<TResult>> func)
        {
            var result = default(TResult);

            try
            {
                var req = new FlurlRequest(_config.BaseUrl + url)
                        .ConfigureRequest(x =>
                        {
                            var jsonSettings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Include,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            };
                            x.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
                        });

                result = func.Invoke(req).Result;
            }
            catch (FlurlHttpException ex)
            {
                throw;
            }

            return result;
        }
    }
}