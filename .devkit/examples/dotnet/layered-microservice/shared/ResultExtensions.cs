using System;
using Microsoft.AspNetCore.Mvc;

namespace LayeredMicroservice.Shared;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result) => result switch
    {
        { IsSuccess: true } => new OkResult(),
        { Code: ErrorCode.Validation, Errors: var errors } => new BadRequestObjectResult(new ProblemDetails
        {
            Status = (int)ErrorCode.Validation,
            Title = ErrorCode.Validation.GetTitle(),
            Detail = string.Join("; ", errors)
        }),
        { Code: ErrorCode.Domain, Errors: var domainErrors } => new ObjectResult(new ProblemDetails
        {
            Status = (int)ErrorCode.Domain,
            Title = ErrorCode.Domain.GetTitle(),
            Detail = string.Join("; ", domainErrors)
        }) { StatusCode = (int)ErrorCode.Domain },
        { Code: ErrorCode.Cancelled } => new StatusCodeResult((int)ErrorCode.Cancelled),
        _ => new ObjectResult(new ProblemDetails
        {
            Status = (int)ErrorCode.Unexpected,
            Title = ErrorCode.Unexpected.GetTitle()
        }) { StatusCode = (int)ErrorCode.Unexpected }
    };

    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, object> map) =>
        result.IsSuccess
            ? new OkObjectResult(map(result.Value!))
            : ((Result)result).ToActionResult();

    public static IActionResult ToActionResult(this ControllerBase controller, Result result) =>
        result.ToActionResult();

    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result, Func<T, object> map) =>
        result.ToActionResult(map);
}
