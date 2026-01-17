using System.Security.Claims;
using Blazored.LocalStorage;
using MAAME.DROMO.PARTOGRAPH.MONITORING.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace MAAME.DROMO.PARTOGRAPH.MONITORING.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private const string SessionKey = "monitoring_session";
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var session = await _localStorage.GetItemAsync<UserSession>(SessionKey);

                if (session == null || session.ExpiresAt < DateTime.UtcNow)
                {
                    return new AuthenticationState(_anonymous);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, session.User.ID.ToString()),
                    new Claim(ClaimTypes.Name, session.User.FullName),
                    new Claim(ClaimTypes.Email, session.User.Email),
                    new Claim(ClaimTypes.Role, session.User.Role),
                    new Claim("AccessLevel", session.User.AccessLevel)
                };

                if (session.User.RegionID.HasValue)
                    claims.Add(new Claim("RegionID", session.User.RegionID.Value.ToString()));

                if (session.User.DistrictID.HasValue)
                    claims.Add(new Claim("DistrictID", session.User.DistrictID.Value.ToString()));

                var identity = new ClaimsIdentity(claims, "jwt");
                var principal = new ClaimsPrincipal(identity);

                return new AuthenticationState(principal);
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
