using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using softaware.Authentication.Basic.AspNetCore.AuthorizationProvider;
using softaware.Authentication.Basic.Client;
using Xunit;

namespace softaware.Authentication.Basic.AspNetCore.Test
{
    public class MiddlewareTest
    {
        [Theory]
        [InlineData("api/test")]
        [InlineData("api/test?query=test")]
        [InlineData("api/test?query=test test")]
        [InlineData("api/test?query=test+test")]
        [InlineData("api/test?query=test%20test")]
        public Task Request_MemoryProvider_Authorized(string requestUri)
        {
            return this.TestRequestAsync(
                new MemoryBasicAuthenticationProvider(new Dictionary<string, string>() { { "username", "password" } }),
                "username",
                "password",
                HttpStatusCode.OK,
                requestUri);
        }

        [Theory]
        [InlineData("username", "wrongPassword")]
        [InlineData("wrongUsername", "password")]
        [InlineData("wrongUsername", "wrongPassword")]
        public Task Request_MemoryProvider_Unauthorized(string username, string password)
        {
            return this.TestRequestAsync(
                new MemoryBasicAuthenticationProvider(new Dictionary<string, string>() { { "username", "password" } }),
                username,
                password,
                HttpStatusCode.Unauthorized);
        }

        [Fact]
        public Task Request_SecureMemoryProvider_Authorized()
        {
            return this.TestRequestAsync(
                new SecureMemoryBasicAuthenticationProvider(new Dictionary<string, string>() { { "username", "password" } }),
                "username",
                "password",
                HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("username", "wrongPassword")]
        [InlineData("wrongUsername", "password")]
        [InlineData("wrongUsername", "wrongPassword")]
        public Task Request_SecureMemoryProvider_Unauthorized(string username, string password)
        {
            return this.TestRequestAsync(
                new SecureMemoryBasicAuthenticationProvider(new Dictionary<string, string>() { { "username", "password" } }),
                username,
                password,
                HttpStatusCode.Unauthorized);
        }

        private async Task TestRequestAsync(
            IBasicAuthorizationProvider basicAuthorizationProvider,
            string username,
            string password,
            HttpStatusCode expectedStatusCode,
            string requestUri = "api/test")
        {
            using (var client = this.GetHttpClient(
                new SecureMemoryBasicAuthenticationProvider(new Dictionary<string, string>() { { "username", "password" } }),
                username,
                password))
            {
                var response = await client.GetAsync(requestUri);
                Assert.True(response.StatusCode == expectedStatusCode);
            }
        }

        private HttpClient GetHttpClient(
            IBasicAuthorizationProvider basicAuthorizationProvider, string username, string password)
        {
            var factory = new TestWebApplicationFactory(basicAuthorizationProvider);
            return factory.CreateDefaultClient(new BasicAuthenticationDelegatingHandler(username, password));
        }
    }
}
