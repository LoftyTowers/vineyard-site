using System.Collections.Concurrent;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VineyardApi.Middleware;

namespace VineyardApi.Tests.Middleware;

public class RequestLoggingMiddlewareTests
{
    [Test]
    public async Task Logs_information_for_successful_request()
    {
        var loggerProvider = new TestLoggerProvider();
        var logger = CreateLogger(loggerProvider);
        var middleware = new RequestLoggingMiddleware(_ =>
        {
            // default 200
            return Task.CompletedTask;
        }, logger);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/";

        await middleware.InvokeAsync(context);

        var completed = loggerProvider.CompletedEntry();
        completed.Level.Should().Be(LogLevel.Information);
        completed.Exception.Should().BeNull();
        completed.GetStateValue("StatusCode").Should().Be(StatusCodes.Status200OK);
        completed.GetStateValue("RequestMethod").Should().Be("GET");
        completed.GetStateValue("RequestPath").Should().BeOfType<PathString>().Which.Value.Should().Be("/");
    }

    [Test]
    public async Task Logs_warning_for_client_error()
    {
        var loggerProvider = new TestLoggerProvider();
        var logger = CreateLogger(loggerProvider);
        var middleware = new RequestLoggingMiddleware(context =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }, logger);

        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        var completed = loggerProvider.CompletedEntry();
        completed.Level.Should().Be(LogLevel.Warning);
        completed.Exception.Should().BeNull();
        completed.GetStateValue("StatusCode").Should().Be(StatusCodes.Status404NotFound);
    }

    [Test]
    public async Task Logs_error_and_exception_for_server_error()
    {
        var loggerProvider = new TestLoggerProvider();
        var logger = CreateLogger(loggerProvider);
        var middleware = new RequestLoggingMiddleware(_ => throw new InvalidOperationException("boom"), logger);

        var context = new DefaultHttpContext();
        var act = async () => await middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>();

        var completed = loggerProvider.CompletedEntry();
        completed.Level.Should().Be(LogLevel.Error);
        completed.Exception.Should().BeOfType<InvalidOperationException>();
        completed.GetStateValue("StatusCode").Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Test]
    public async Task Logs_warning_without_exception_for_bad_http_request()
    {
        var loggerProvider = new TestLoggerProvider();
        var logger = CreateLogger(loggerProvider);
        var middleware = new RequestLoggingMiddleware(_ =>
        {
            throw new BadHttpRequestException("bad request", StatusCodes.Status400BadRequest);
        }, logger);

        var context = new DefaultHttpContext();
        var act = async () => await middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<BadHttpRequestException>();

        var completed = loggerProvider.CompletedEntry();
        completed.Level.Should().Be(LogLevel.Warning);
        completed.Exception.Should().BeNull();
        completed.GetStateValue("StatusCode").Should().Be(StatusCodes.Status400BadRequest);
    }

    private static ILogger<RequestLoggingMiddleware> CreateLogger(TestLoggerProvider provider)
    {
        var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        return factory.CreateLogger<RequestLoggingMiddleware>();
    }

    private sealed class TestLoggerProvider : ILoggerProvider
    {
        public ConcurrentBag<TestLogEntry> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new TestLogger(categoryName, Entries);

        public void Dispose()
        {
        }

        public TestLogEntry CompletedEntry() =>
            Entries.Single(e => e.Message.StartsWith("Completed"));
    }

    private sealed class TestLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ConcurrentBag<TestLogEntry> _entries;

        public TestLogger(string categoryName, ConcurrentBag<TestLogEntry> entries)
        {
            _categoryName = categoryName;
            _entries = entries;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _entries.Add(new TestLogEntry(logLevel, formatter(state, exception), exception, state, _categoryName));
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();
            public void Dispose()
            {
            }
        }
    }

    private sealed record TestLogEntry(LogLevel Level, string Message, Exception? Exception, object? State, string CategoryName)
    {
        public object? GetStateValue(string key)
        {
            if (State is not IEnumerable<KeyValuePair<string, object?>> pairs)
            {
                return null;
            }

            return pairs.LastOrDefault(kvp => string.Equals(kvp.Key, key, StringComparison.Ordinal)).Value;
        }
    }
}
