// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using graph_tutorial.TokenStorage;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System;

namespace graph_tutorial.Helpers
{
    public static class GraphHelper
    {
        public static async Task<User> GetUserDetailsAsync(string accessToken)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", accessToken);
                    }));

            return await graphClient.Me.Request().GetAsync();
        }

        public static async Task<bool> UpdatePassword()
        {
            var passwordChange = false;

            try
            {
                var graphClient = GetAuthenticatedClient();
                await graphClient.Me.ChangePassword("OldPassword", "NewPassword").Request().PostAsync();
                passwordChange = true;
            }
            catch (Exception ex)
            {
                passwordChange = false;
            }
            return passwordChange;
        }

        // Load configuration settings from PrivateSettings.config
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string graphScopes = ConfigurationManager.AppSettings["ida:AppScopes"];

        public static async Task<Contact> CreateContact()
        {
            var graphClient = GetAuthenticatedClient();

            var contact = new Contact
            {
                GivenName = "Pavel1",
                Surname = "Bansky1",
                EmailAddresses = new List<EmailAddress>()
                {
        new EmailAddress
                {
            Address = "pavelb@fabrikam.onmicrosoft.com",
            Name = "Pavel Bansky"
                }
                },
                BusinessPhones = new List<String>()
                {
        "+1 732 555 0102"
                }
            };

            await graphClient.Me.Contacts
                .Request()
                .AddAsync(contact);

            return contact;
        }
        public static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            var graphClient = GetAuthenticatedClient();

            var events = await graphClient.Me.Events.Request()
                .Select("subject,organizer,start,end")
                .OrderBy("createdDateTime DESC")
                .GetAsync();

            return events.CurrentPage;
        }

        public static async Task<IEnumerable<Contact>> GetContacts()
        {
            var graphClient = GetAuthenticatedClient();

            var contacts = await graphClient.Me.Contacts.Request()
                .Select("displayname,mobilePhone,birthday")
                .OrderBy("createdDateTime DESC")
                .GetAsync();

            return contacts.CurrentPage;
        }


        private static GraphServiceClient GetAuthenticatedClient()
        {
            return new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var idClient = ConfidentialClientApplicationBuilder.Create(appId)
                            .WithRedirectUri(redirectUri)
                            .WithClientSecret(appSecret)
                            .Build();

                        string signedInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                        var tokenStore = new SessionTokenStore(signedInUserId, HttpContext.Current);
                        tokenStore.Initialize(idClient.UserTokenCache);

                        var accounts = await idClient.GetAccountsAsync();

                // By calling this here, the token can be refreshed
                // if it's expired right before the Graph call is made
                var scopes = graphScopes.Split(' ');
                        var result = await idClient.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                            .ExecuteAsync();

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));
        }
    }
}