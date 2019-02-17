using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using softaware.Authentication.Hmac.Client;
using Xunit;

namespace softaware.Authentication.Hmac.AspNetCore.Test
{
    public class MiddlewareTest
    {
        [Theory]
        [InlineData("api/test")]
        [InlineData("api/test?query=test")]
        [InlineData("api/test?query=test test")]
        [InlineData("api/test?query=test+test")]
        [InlineData("api/test?query=test%20test")]
        public Task Request_Authorized(string requestUri)
        {
            return this.TestRequestAsync(
                new Dictionary<string, string>() { { "appId", "MNpx/353+rW+pqv8UbRTAtO1yoabl8/RFDAv/615u5w=" } },
                "appId",
                "MNpx/353+rW+pqv8UbRTAtO1yoabl8/RFDAv/615u5w=",
                HttpStatusCode.OK,
                requestUri);
        }

        [Theory]
        [InlineData("appId", "YXJld3JzZHJkc2FhcndlZQ==")]
        [InlineData("wrongAppId", "MNpx/353+rW+pqv8UbRTAtO1yoabl8/RFDAv/615u5w=")]
        public Task Request_Unauthorized(string appId, string apiKey)
        {
            return this.TestRequestAsync(
                new Dictionary<string, string>() { { "appId", "MNpx/353+rW+pqv8UbRTAtO1yoabl8/RFDAv/615u5w=" } },
                appId,
                apiKey,
                HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("appId", "MNpx/353+rW+sdf/RFDAv/615u5w=")]
        [InlineData("wrongAppId", "MNpx/353+rW+sdf/RFDAv/615u5w=")]
        public Task Request_ApiKeyBadFormat_ThrowsException(string appId, string apiKey)
        {
            return Assert.ThrowsAsync<ArgumentException>(() => this.TestRequestAsync(
                new Dictionary<string, string>() { { "appId", "MNpx/353+rW+pqv8UbRTAtO1yoabl8/RFDAv/615u5w=" } },
                appId,
                apiKey,
                HttpStatusCode.Unauthorized));
        }

        private async Task TestRequestAsync(
            IDictionary<string, string> authenticatedApps,
            string appId,
            string apiKey,
            HttpStatusCode expectedStatusCode,
            string requestUri = "api/test")
        {
            using (var client = this.GetHttpClient(
                authenticatedApps,
                appId,
                apiKey))
            {
                var response = await client.GetAsync(requestUri);
                Assert.True(response.StatusCode == expectedStatusCode);
            }
        }

        private HttpClient GetHttpClient(IDictionary<string, string> hmacAuthenticatedApps, string appId, string apiKey)
        {
            var factory = new TestWebApplicationFactory(hmacAuthenticatedApps);
            return factory.CreateDefaultClient(new ApiKeyDelegatingHandler(appId, apiKey));
        }
    }
}
