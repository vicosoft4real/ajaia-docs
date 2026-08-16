using System.Text.Json;
using AjaiaDocs.Api.Common;
using AjaiaDocs.Api.Middleware;
using AjaiaDocs.Application;
using AjaiaDocs.Application.Features.Import;
using AjaiaDocs.Infrastructure;
using AjaiaDocs.Infrastructure.Data.Migrations;
using Carter;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAjaiaDocsApplication();
builder.Services.AddAjaiaDocsInfrastructure(builder.Configuration);
builder.Services.AddCarter();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentActor>();
builder.Services.AddSingleton<ResultHttpMapper>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddAuthorization();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = StrictTextImportParser.MaxFileBytes;
});
builder.Services.Configure<RouteHandlerOptions>(options =>
{
    options.ThrowOnBadRequest = true;
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        // Render terminates TLS before forwarding the request to Kestrel.
        // Trust only the nearest proxy's scheme; client IP and host stay untouched.
        options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AjaiaDocs.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context => WriteAuthenticationProblemAsync(context,
            "unauthenticated", "An authenticated session is required.",
            StatusCodes.Status401Unauthorized);
        options.Events.OnRedirectToAccessDenied = context => WriteAuthenticationProblemAsync(
            context, "forbidden", "The session cannot perform this action.",
            StatusCodes.Status403Forbidden);
    });

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "AjaiaDocs.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}

app.UseMiddleware<ApiBindingFailureMiddleware>();
app.UseExceptionHandler(_ => { });
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<SchemaMigrationRunner>()
        .MigrateAsync(CancellationToken.None);
}

app.MapCarter();
app.MapFallbackToFile("index.html");

app.Run();

static Task WriteAuthenticationProblemAsync(RedirectContext<CookieAuthenticationOptions> context,
    string code, string detail, int statusCode) =>
    ResultHttpMapper.Problem(code, detail, statusCode).ExecuteAsync(context.HttpContext);

public partial class Program;
