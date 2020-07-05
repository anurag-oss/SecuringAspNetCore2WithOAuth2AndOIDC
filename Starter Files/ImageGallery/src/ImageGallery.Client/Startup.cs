using IdentityModel;
using ImageGallery.Client.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Before
            //Claim type: sid - Claim value: 737f341c029bf2f86cf2bc5b85a2842e
            //Claim type: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier - Claim value: BD23DFD5-9192-49AE-880A-9F0C71FB14CF
            //Claim type: http://schemas.microsoft.com/identity/claims/identityprovider - Claim value: local
            //Claim type: http://schemas.microsoft.com/claims/authnmethodsreferences - Claim value: pwd
            //Claim type: given_name - Claim value: Frank
            //Claim type: family_name - Claim value: Underwood

            // After
            //Claim type: sid - Claim value: 1cc9150a35d42ec0e07aca2ee0a9e2ab
            //Claim type: sub - Claim value: BD23DFD5 - 9192 - 49AE - 880A - 9F0C71FB14CF
            //Claim type: idp - Claim value: local
            //Claim type: given_name - Claim value: Frank
            //Claim type: family_name - Claim value: Underwood
        }
 
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();

            services
                .AddAuthentication( authenticationOptions =>
                {
                    // There is a overloaded AddAuthentication method which just takes the name of default scheme
                    // By using AuthencticationOptions we can use different default schemes for different actions like
                    // challenge, sign-in , sign-out, forbid
                    authenticationOptions.DefaultScheme = "Cookies";
                    authenticationOptions.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", oidcConnectOptions => 
                {
                    oidcConnectOptions.SignInScheme = "Cookies";
                    oidcConnectOptions.Authority = "https://localhost:44379/"; // The base url from where one can find the discovery and eventually other endpoints
                    oidcConnectOptions.ClientId = "imagegalleryclient";
                    oidcConnectOptions.ResponseType = "code id_token"; // Corresponds to hybrid flow
                    //oidcConnectOptions.CallbackPath = new PathString("...") Use default [signin-oidc"], no need to override
                    //oidcConnectOptions.SignedOutCallbackPath = new PathString("...") Use default[signout-callback-oidc], no need to override
                    oidcConnectOptions.Scope.Add("openid");
                    oidcConnectOptions.Scope.Add("profile");
                    oidcConnectOptions.SaveTokens = true; // The tokens will be saved in the properties section of cookie
                    oidcConnectOptions.ClientSecret = "secret";
                    oidcConnectOptions.GetClaimsFromUserInfoEndpoint = true;

                    // Remove the filter which was removing the amr claim
                    // and remove the sid and idp claims
                    oidcConnectOptions.ClaimActions.Remove("amr");
                    oidcConnectOptions.ClaimActions.DeleteClaim("sid");
                    oidcConnectOptions.ClaimActions.DeleteClaim("idp");
                    //
                    //Claim type: sub - Claim value: BD23DFD5 - 9192 - 49AE - 880A - 9F0C71FB14CF
                    //Claim type: amr - Claim value: pwd
                    //Claim type: given_name - Claim value: Frank
                    //Claim type: family_name - Claim value: Underwood
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
