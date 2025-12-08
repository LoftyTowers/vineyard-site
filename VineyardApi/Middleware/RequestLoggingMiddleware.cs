using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;

namespace VineyardApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private const string CorrelationHeader = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = ResolveCorrelationId(context);
            context.Items[CorrelationHeader] = correlationId;
            context.Response.Headers[CorrelationHeader] = correlationId;

            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestId"] = context.TraceIdentifier,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method
            });

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting {RequestMethod} {RequestPath}");

            try
            {
                await _next(context);
                stopwatch.Stop();
                _logger.LogInformation("Completed {RequestMethod} {RequestPath} with {StatusCode} in {ElapsedMilliseconds} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "Unhandled exception during {RequestMethod} {RequestPath} after {ElapsedMilliseconds} ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        private static string ResolveCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationHeader, out var values) && !string.IsNullOrWhiteSpace(values))
            {
                return values.ToString();
            }

            return context.TraceIdentifier ?? Guid.NewGuid().ToString();
        }
    }
}
