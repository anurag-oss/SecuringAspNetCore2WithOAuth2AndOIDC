using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

using System.Collections.Generic;
using System.Security.Claims;

namespace Marvin.IDP
{
    public static class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                    Username = "Frank",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Main Road 1"),
                        new Claim("role", "FreeUser"),
                        new Claim("country", "nl"),
                        new Claim("subscriptionLevel", "FreeUser"),
                    }
                },

                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Claire",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Big Street 2"),
                        new Claim("role", "PayingUser"),
                        new Claim("country", "be"),
                        new Claim("subscriptionLevel", "PayingUser"),
                    }
                }
            };
        }

        // Indentity-related resources (scopes)
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(), // SubjectId is returned
                new IdentityResources.Profile(), // Profile is returned
                new IdentityResources.Address(), // Address resource can be returned by IdentityServer4 for any client

                new IdentityResource("roles", "Your role(s)", new List<string> { "role" }),
                new IdentityResource("country", "Your country of residence", new List<string> { "country" }),
                new IdentityResource("subscriptionLevel", "Your subscription level", new List<string> { "subscriptionLevel" }),
            };
        }

        // Api related resources scopes
        public static IEnumerable<ApiResource> GetApiResource()
        {
            return new List<ApiResource>
            {
                // The access token will contain role claims
                new ApiResource("imagegalleryapi", "Image Gallery API", new List<string> { "role" }) 
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Hybrid, // We are using hybrid flow
                    RedirectUris = new List<string>
                    {
                        "https://localhost:44355/signin-oidc"
                    },
                    PostLogoutRedirectUris = new  List<string>
                    {
                        "https://localhost:44355/signout-callback-oidc"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address, // The particular client is allowed to ask for address info
                        "roles",
                        "imagegalleryapi",
                        "country",
                        "subscriptionLevel",


                    },
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    }
                }
            };
        }
    }
}
