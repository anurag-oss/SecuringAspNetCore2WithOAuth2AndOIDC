using IdentityModel.Client;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpClient _httpClient = new HttpClient();

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpClient> GetClient()
        {
            string accessToken = string.Empty;

            var currentHttpContext = _httpContextAccessor.HttpContext;
            
            var expires_at = await currentHttpContext.GetTokenAsync("expires_at");
            if (string.IsNullOrWhiteSpace(expires_at) || 
                ((DateTime.Parse(expires_at).AddSeconds(-60)).ToUniversalTime() < DateTime.UtcNow))
            {
                // Renew if access token is about to expire in 60 seconds
                accessToken = await this.RenewTokens();
            }
            else
            {
                accessToken = await currentHttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                _httpClient.SetBearerToken(accessToken);
            }

            _httpClient.BaseAddress = new Uri("https://localhost:44351/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return _httpClient;
        }

        private async Task<string> RenewTokens()
        {
            // Get the current HttpContext to get the existing refresh token
            var currentHttpContext = _httpContextAccessor.HttpContext;
            var currentRefreshToken = await currentHttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            // Meta data from the discovery endpoint
            var discoveryClient = new DiscoveryClient("https://localhost:44379");
            var metaDataResponse = await discoveryClient.GetAsync();

            // A new token client to get new tokens
            var tokenClient = new TokenClient(metaDataResponse.TokenEndpoint, "imagegalleryclient", "secret");

            // get the new gamut of tokens
            var tokenResponse = await tokenClient.RequestRefreshTokenAsync(currentRefreshToken);

            if (!tokenResponse.IsError)
            {
                var updatedTokens = new List<AuthenticationToken>
                {
                    new AuthenticationToken
                    {
                        Name = OpenIdConnectParameterNames.IdToken,
                        Value = tokenResponse.IdentityToken,
                    },
                    new AuthenticationToken
                    {
                        Name = OpenIdConnectParameterNames.AccessToken,
                        Value = tokenResponse.AccessToken,
                    },
                    new AuthenticationToken
                    {
                        Name = OpenIdConnectParameterNames.RefreshToken,
                        Value = tokenResponse.RefreshToken,
                    },
                    new AuthenticationToken
                    {
                        // A neat trick to store the expiry
                        Name = "expires_at",
                        Value = (DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn)).ToString("o", CultureInfo.InvariantCulture),
                    }
                };

                // Get current Principal and Properties
                var currentAuthenticationResult = await currentHttpContext.AuthenticateAsync("Cookies");
                currentAuthenticationResult.Properties.StoreTokens(updatedTokens);

                // update the cookie
                await currentHttpContext.SignInAsync("Cookies", currentAuthenticationResult.Principal, currentAuthenticationResult.Properties);

                return tokenResponse.AccessToken;
            }
            else
            {
                throw new Exception("Problem encountered while refreshing tokens.", tokenResponse.Exception);
            }
        }
    }
}

