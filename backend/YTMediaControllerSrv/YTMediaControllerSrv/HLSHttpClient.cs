using System;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace YTMediaControllerSrv
{
    internal class HLSHttpClient
    {
        private readonly HttpClient _httpClient;

        public HLSHttpClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
            return await SendWithRetryAsync(() => _httpClient.GetAsync(url));
        }

        public async Task<HttpResponseMessage> Send(Func<HttpRequestMessage> requestFactory, HttpCompletionOption option)
        {
            return await SendWithRetryAsync(() => _httpClient.SendAsync(requestFactory(), option));
        }

        private async Task<HttpResponseMessage> SendWithRetryAsync(Func<Task<HttpResponseMessage>> sendFunc)
        {
            const int maxRetries = 3;
            const int initialDelayMs = 500;
            const int maxDelayMs = 5000;
            int attempt = 0;
            Exception lastException = null;

            while (attempt < maxRetries)
            {
                try
                {
                    var response = await sendFunc();
                    if (response.IsSuccessStatusCode)
                        return response;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
                attempt++;
                int delay = Math.Min(initialDelayMs * attempt, maxDelayMs);
                await Task.Delay(delay);
            }

            if (lastException != null)
                throw lastException;

            return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
            {
                ReasonPhrase = "Max retry attempts exceeded"
            };
        }
    }
}
