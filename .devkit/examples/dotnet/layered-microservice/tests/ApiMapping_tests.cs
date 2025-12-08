using System;
using LayeredMicroservice.Shared;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace LayeredMicroservice.Tests;

public sealed class ApiMappingTests
{
    [Test]
    public void ToActionResult_WhenSuccess_Returns200()
    {
        var result = Result.Success();
        var action = result.ToActionResult();
        Assert.That((action as OkResult)?.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public void ToActionResult_WhenValidationError_Returns400()
    {
        var result = Result.Failure(ErrorCode.Validation, new[] { "Name is required" });
        var action = result.ToActionResult();
        Assert.That((action as BadRequestObjectResult)?.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void ToActionResult_WhenDomainError_Returns422()
    {
        var result = Result.Failure(ErrorCode.Domain, new[] { "Quantity must be positive" });
        var action = result.ToActionResult();
        Assert.That((action as ObjectResult)?.StatusCode, Is.EqualTo(422));
    }

    [Test]
    public void ToActionResult_WhenCancelled_Returns499()
    {
        var result = Result.Failure(ErrorCode.Cancelled, Array.Empty<string>());
        var action = result.ToActionResult();
        Assert.That((action as StatusCodeResult)?.StatusCode, Is.EqualTo(499));
    }

    [Test]
    public void ToActionResult_WhenUnexpected_Returns500()
    {
        var result = Result.Failure(ErrorCode.Unexpected, new[] { "boom" });
        var action = result.ToActionResult();
        Assert.That((action as ObjectResult)?.StatusCode, Is.EqualTo(500));
    }
}
