using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Marvin.IDP
{
    public class Startup
    {
        // The discovery endpint is at .well-known/openid-configuration
        // For Azure AD
        // https://login.microsoftonline.com/<tenant_name>.onmicrosoft.com/.well-known/openid-configuration
        // For Localhost
        // localhost/.well-known/openid-configuration
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services
                .AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddTestUsers(Config.GetUsers())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResource())
                .AddInMemoryClients(Config.GetClients());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

        }
    }
}
