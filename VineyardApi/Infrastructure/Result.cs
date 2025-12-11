using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VineyardApi.Infrastructure;

public enum ErrorCode
{
    None,
    BadRequest,
    NotFound,
    Validation,
    ClientClosedRequest,
    InternalError,
    Domain,
    Unauthorized,
    Unexpected
}

public class Result
{
    protected Result(bool isSuccess, ErrorCode errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Error = isSuccess ? global::VineyardApi.Infrastructure.ErrorCode.None : errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    // Alias for backward-compatibility with the services/tests.
    public ErrorCode Error { get; }

    public ErrorCode? Code => IsSuccess ? null : Error;

    // Alias to preserve the previous Infrastructure name.
    public ErrorCode? ErrorCode => Code;

    public string? ErrorMessage { get; }

    public static Result Success() => new(true, global::VineyardApi.Infrastructure.ErrorCode.None, null);

    public static Result Failure(ErrorCode errorCode, string? message = null) => new(false, errorCode, message);
}

public class Result<T> : Result
{
    private Result(T value) : base(true, global::VineyardApi.Infrastructure.ErrorCode.None, null)
    {
        Value = value;
    }

    private Result(ErrorCode errorCode, string? message) : base(false, errorCode, message)
    {
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(ErrorCode errorCode, string? message = null) => new(errorCode, message);
}

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok();
        }

        return BuildProblemResult(result, controller);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            if (result.Value is null)
            {
                return controller.Ok();
            }

            return controller.Ok(result.Value);
        }

        return BuildProblemResult(result, controller);
    }

    private static IActionResult BuildProblemResult(Result result, ControllerBase controller)
    {
        var (statusCode, title) = GetStatusAndTitle(result.Error);
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = result.ErrorMessage ?? title,
        };
        problem.Extensions["errorCode"] = result.Error.ToString();

        return controller.StatusCode(statusCode, problem);
    }

    private static (int StatusCode, string Title) GetStatusAndTitle(ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.BadRequest => (StatusCodes.Status400BadRequest, "Bad request"),
            ErrorCode.Domain => (StatusCodes.Status400BadRequest, "Domain error"),
            ErrorCode.Validation => (StatusCodes.Status422UnprocessableEntity, "Validation failed"),
            ErrorCode.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorCode.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
            ErrorCode.ClientClosedRequest => (499, "Client closed request"),
            ErrorCode.Unexpected or ErrorCode.InternalError => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }
}
