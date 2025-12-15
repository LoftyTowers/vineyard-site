using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Runtime.ExceptionServices;

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
            _logger.LogInformation("Starting {RequestMethod} {RequestPath}", context.Request.Method, context.Request.Path);

            Exception? exception = null;
            ExceptionDispatchInfo? dispatchInfo = null;

            try
            {
                await _next(context);
            }
            catch (BadHttpRequestException ex)
            {
                exception = ex;
                dispatchInfo = ExceptionDispatchInfo.Capture(ex);
                context.Response.StatusCode = ex.StatusCode;
            }
            catch (Exception ex)
            {
                exception = ex;
                dispatchInfo = ExceptionDispatchInfo.Capture(ex);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }

            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var logLevel = ResolveLogLevel(statusCode);

            const string completionMessage = "Completed {RequestMethod} {RequestPath} with {StatusCode} in {ElapsedMilliseconds} ms";

            if (logLevel == LogLevel.Error && exception is not null)
            {
                _logger.Log(logLevel, exception, completionMessage,
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.Log(logLevel, completionMessage,
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds);
            }

            dispatchInfo?.Throw();
        }

        private static string ResolveCorrelationId(HttpContext context)
        {
            try
            {
                if (context.Request.Headers.TryGetValue(CorrelationHeader, out var values) && !string.IsNullOrWhiteSpace(values))
                {
                    return values.ToString();
                }

                return context.TraceIdentifier ?? Guid.NewGuid().ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static LogLevel ResolveLogLevel(int statusCode)
        {
            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                return LogLevel.Error;
            }

            if (statusCode >= StatusCodes.Status400BadRequest)
            {
                return LogLevel.Warning;
            }

            return LogLevel.Information;
        }
    }
}
