using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AjaiaDocs.Api.Common;
using AjaiaDocs.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AjaiaDocs.IntegrationTests.Api;

[Collection(PostgresCollection.CollectionName)]
public sealed class SessionEndpointsTests(PostgresFixture postgres) : IDisposable
{
    private readonly AjaiaDocsWebApplicationFactory _factory = new(postgres);

    [Fact]
    public async Task Login_requires_antiforgery_and_issues_only_the_session_cookie()
    {
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        var missingToken = await client.PostAsJsonAsync("/api/session",
            new { userId = DemoUsers.AminaId });
        var loggedIn = await client.SendWithAntiforgeryAsync(HttpMethod.Post,
            "/api/session", new { userId = DemoUsers.AminaId });

        Assert.Equal(HttpStatusCode.BadRequest, missingToken.StatusCode);
        Assert.Equal("antiforgery_validation_failed",
            (await missingToken.Content.ReadFromJsonAsync<ProblemResponse>())!.Code);
        Assert.Equal(HttpStatusCode.OK, loggedIn.StatusCode);
        Assert.Contains(loggedIn.Headers.GetValues("Set-Cookie"), value =>
            value.StartsWith("AjaiaDocs.Session=", StringComparison.Ordinal) &&
            value.Contains("httponly", StringComparison.OrdinalIgnoreCase) &&
            value.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase));

        using var body = JsonDocument.Parse(await loggedIn.Content.ReadAsStringAsync());
        Assert.Equal(DemoUsers.AminaId,
            body.RootElement.GetProperty("id").GetGuid());
        Assert.Equal("#365CF5", body.RootElement.GetProperty("avatarColor").GetString());
    }

    [Fact]
    public async Task Session_get_is_anonymous_after_logout()
    {
        var client = await _factory.CreateAuthenticatedClientAsync(DemoUsers.ChidiId);
        var session = await client.GetAsync("/api/session");
        var logout = await client.SendWithAntiforgeryAsync(HttpMethod.Delete, "/api/session");
        var afterLogout = await client.GetAsync("/api/session");

        Assert.Equal(HttpStatusCode.OK, session.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }

    [Fact]
    public async Task Unknown_seeded_identity_is_not_accepted()
    {
        var client = _factory.CreateClient();
        var response = await client.SendWithAntiforgeryAsync(HttpMethod.Post,
            "/api/session", new { userId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("not_found",
            (await response.Content.ReadFromJsonAsync<ProblemResponse>())!.Code);
    }

    [Fact]
    public void Production_cookie_is_secure_strict_and_eight_hour_sliding()
    {
        using var factory = new AjaiaDocsWebApplicationFactory(postgres, "Production");
        var options = factory.Services
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal("AjaiaDocs.Session", options.Cookie.Name);
        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(SameSiteMode.Strict, options.Cookie.SameSite);
        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.Equal(TimeSpan.FromHours(8), options.ExpireTimeSpan);
        Assert.True(options.SlidingExpiration);
        Assert.True(factory.Services.GetRequiredService<IOptions<RouteHandlerOptions>>()
            .Value.ThrowOnBadRequest);
    }

    public void Dispose() => _factory.Dispose();
}
