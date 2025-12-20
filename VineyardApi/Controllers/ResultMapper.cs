using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;

namespace VineyardApi.Controllers
{
    public static class ResultMapper
    {
        public static IActionResult FromValidationResult(ControllerBase controller, ValidationResult validationResult)
        {
            var errors = validationResult.Errors
                .Select(e => new ValidationError(e.PropertyName, e.ErrorMessage))
                .ToArray();

            return ToActionResult(controller, Result.Failure(ErrorCode.Validation, "Validation failed", errors));
        }

        public static IActionResult ToActionResult(ControllerBase controller, Result result)
        {
            if (result.Success)
            {
                return controller.Ok();
            }

            return CreateErrorResult(controller, result);
        }

        public static IActionResult ToActionResult<T>(ControllerBase controller, Result<T> result)
        {
            if (result.Success)
            {
                return controller.Ok(result.Value);
            }

            return CreateErrorResult(controller, result);
        }

        private static IActionResult CreateErrorResult(ControllerBase controller, Result result)
        {
            return result.ErrorCode switch
            {
                ErrorCode.Validation => controller.BadRequest(new ValidationProblemDetails(result.ValidationErrors
                    .GroupBy(e => e.Field)
                    .ToDictionary(g => g.Key, g => g.Select(v => v.Message).ToArray()))),
                ErrorCode.Domain => BuildProblem(controller, StatusCodes.Status422UnprocessableEntity, result, "Domain error"),
                ErrorCode.NotFound => BuildProblem(controller, StatusCodes.Status404NotFound, result, "Not found"),
                ErrorCode.BadRequest => BuildProblem(controller, StatusCodes.Status400BadRequest, result, "Bad request"),
                ErrorCode.Unauthorized => controller.Unauthorized(),
                ErrorCode.Forbidden => controller.Forbid(),
                ErrorCode.Conflict => controller.Conflict(result.Message),
                ErrorCode.Cancelled => BuildProblem(controller, 499, result, "Request was cancelled"),
                ErrorCode.Unknown or ErrorCode.Unexpected => BuildProblem(controller, StatusCodes.Status500InternalServerError, result, "An unexpected error occurred"),
                _ => BuildProblem(controller, StatusCodes.Status422UnprocessableEntity, result, "Domain error")
            };
        }

        private static IActionResult BuildProblem(ControllerBase controller, int statusCode, Result result, string fallbackMessage)
        {
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = result.Message ?? fallbackMessage
            };
            problem.Extensions["errorCode"] = result.ErrorCode.ToString();
            return controller.StatusCode(statusCode, problem);
        }
    }
}
