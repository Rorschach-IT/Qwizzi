using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Qwizzi.Api.Security;

namespace Qwizzi.UnitTests;

public sealed class CloudflareTurnstileVerifierTests
{
    [Fact]
    public async Task VerifyAsyncAcceptsOnlyMatchingActionAndHostname()
    {
        using var client = CreateClient("""{"success":true,"action":"register","hostname":"qwizzi.example"}""");
        var verifier = CreateVerifier(client);

        var result = await verifier.VerifyAsync(
            "valid-token",
            "register",
            "qwizzi.example",
            CancellationToken.None);

        Assert.Equal(TurnstileVerificationStatus.Verified, result);
    }

    [Theory]
    [InlineData("login", "qwizzi.example")]
    [InlineData("register", "attacker.example")]
    public async Task VerifyAsyncRejectsMismatchedActionOrHostname(
        string action,
        string hostname)
    {
        using var client = CreateClient(
            $$"""{"success":true,"action":"{{action}}","hostname":"{{hostname}}"}""");
        var verifier = CreateVerifier(client);

        var result = await verifier.VerifyAsync(
            "valid-token",
            "register",
            "qwizzi.example",
            CancellationToken.None);

        Assert.Equal(TurnstileVerificationStatus.Rejected, result);
    }

    [Fact]
    public async Task VerifyAsyncFailsClosedWhenSecretIsMissing()
    {
        using var client = new HttpClient(new StubHandler(_ =>
            throw new InvalidOperationException("The verifier should not call Cloudflare.")));
        var configuration = new ConfigurationBuilder().Build();
        var verifier = new CloudflareTurnstileVerifier(
            client,
            configuration,
            NullLogger<CloudflareTurnstileVerifier>.Instance);

        var result = await verifier.VerifyAsync(
            "valid-token",
            "register",
            "qwizzi.example",
            CancellationToken.None);

        Assert.Equal(TurnstileVerificationStatus.NotConfigured, result);
    }

    private static CloudflareTurnstileVerifier CreateVerifier(HttpClient client)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DOTNET_TURNSTILE_SECRET_KEY"] = "test-secret"
            })
            .Build();
        return new CloudflareTurnstileVerifier(
            client,
            configuration,
            NullLogger<CloudflareTurnstileVerifier>.Instance);
    }

    private static HttpClient CreateClient(string json) =>
        new(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        }));

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request));
    }
}
