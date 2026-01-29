using Blazored.LocalStorage;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;
using System.Net.Http.Headers;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public AuthorizationMessageHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var session = await _localStorage.GetItemAsync<UserSession>("monitoring_session");

                if (session != null && !string.IsNullOrEmpty(session.Token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
                }
            }
            catch (Exception)
            {
                // If local storage is not available (e.g. pre-rendering), we just don't attach the token
                // The request will likely fail with 401 if the endpoint requires auth
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
