using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

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
                ["RequestMethod"] = context.Request.Method,
                ["QueryString"] = context.Request.QueryString.ToString(),
                ["CorrelationHeader"] = context.Request.Headers[CorrelationHeader].ToString(),
                ["IsAuthenticated"] = context.User?.Identity?.IsAuthenticated ?? false,
                ["UserId"] = ResolveUserId(context.User)
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
            var routeValues = context.GetRouteData()?.Values ?? new RouteValueDictionary();
            var correlationHeader = context.Request.Headers[CorrelationHeader].ToString();
            var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
            var userId = ResolveUserId(context.User);

            const string completionMessage = "Completed {RequestMethod} {RequestPath} with {StatusCode} in {ElapsedMilliseconds} ms (QueryString: {QueryString}, RouteValues: {RouteValues}, Authenticated: {IsAuthenticated}, UserId: {UserId}, CorrelationId: {CorrelationId})";

            if (logLevel == LogLevel.Error && exception is not null)
            {
                _logger.Log(logLevel, exception, completionMessage,
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    context.Request.QueryString.ToString(),
                    routeValues,
                    isAuthenticated,
                    userId,
                    correlationHeader);
            }
            else
            {
                _logger.Log(logLevel, completionMessage,
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    context.Request.QueryString.ToString(),
                    routeValues,
                    isAuthenticated,
                    userId,
                    correlationHeader);
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

        private static string? ResolveUserId(ClaimsPrincipal? user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user?.Identity?.Name;
        }
    }
}
