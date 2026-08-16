using System.Text.Json;
using AjaiaDocs.Api.Common;
using AjaiaDocs.Api.Middleware;
using AjaiaDocs.Core.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AjaiaDocs.UnitTests.Api;

public sealed class HttpFailureBoundaryTests
{
    [Fact]
    public async Task Failure_result_is_logged_but_returns_only_sanitized_problem()
    {
        var logger = new RecordingLogger<ResultHttpMapper>();
        var mapper = new ResultHttpMapper(logger);
        var internalError = new AjaiaError("database_connection_failed",
            "password=correct-horse-battery-staple", ErrorType.Failure);
        var context = CreateHttpContext();

        await mapper.ToHttpResult(Result<bool>.Failure(internalError)).ExecuteAsync(context);

        var body = await ReadBodyAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Equal("unexpected_failure", body.GetProperty("code").GetString());
        Assert.Equal("An unexpected error occurred.", body.GetProperty("detail").GetString());
        Assert.DoesNotContain(internalError.Code, body.GetRawText(), StringComparison.Ordinal);
        Assert.DoesNotContain(internalError.Message, body.GetRawText(), StringComparison.Ordinal);
        Assert.Contains(logger.Messages, message =>
            message.Contains(internalError.Code, StringComparison.Ordinal) &&
            message.Contains(internalError.Message, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Unexpected_exception_response_does_not_expose_exception_details()
    {
        var logger = new RecordingLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger);
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("secret database host db.internal");

        var handled = await handler.TryHandleAsync(context, exception,
            CancellationToken.None);

        var body = await ReadBodyAsync(context);
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("unexpected_failure", body.GetProperty("code").GetString());
        Assert.DoesNotContain(exception.Message, body.GetRawText(), StringComparison.Ordinal);
        Assert.Contains(logger.Messages, message =>
            message.Contains("unexpected API failure", StringComparison.OrdinalIgnoreCase));
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();
        return new DefaultHttpContext
        {
            RequestServices = services,
            Response = { Body = new MemoryStream() }
        };
    }

    private static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        return document.RootElement.Clone();
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));
    }
}
