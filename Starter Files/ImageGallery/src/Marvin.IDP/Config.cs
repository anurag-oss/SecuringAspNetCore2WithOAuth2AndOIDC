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
                    SubjectId = "BD23DFD5-9192-49AE-880A-9F0C71FB14CF",
                    Username = "Frank",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Main Road 1"),
                        new Claim("role", "FreeUser"),
                    }
                },

                new TestUser
                {
                    SubjectId = "76C82514-3E57-4BDA-89F4-CD48DABA5BC4",
                    Username = "Claire",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Big Street 2"),
                        new Claim("role", "PayingUser"),
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
                        "roles"
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
