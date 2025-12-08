using Microsoft.AspNetCore.Mvc;

namespace VineyardApi.Tests
{
    public static class ResultHttpMapper
    {
        public static int? MapToStatusCode(IActionResult result)
        {
            return result switch
            {
                ObjectResult objectResult => objectResult.StatusCode
                    ?? (objectResult.Value as ProblemDetails)?.Status,
                StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
                _ => null
            };
        }
    }
}
