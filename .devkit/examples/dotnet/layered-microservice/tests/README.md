# Tests Layer

**Purpose**
- Demonstrate how to exercise each layer’s contracts using NUnit and Moq.
- Verify HTTP status mapping, use-case outcomes, and adapter error handling.

**Golden Rules**
- Keep tests deterministic by mocking ports, clocks, and external systems.
- Assert on `Result` properties or `IActionResult` status codes, not implementation details.
- Cover happy paths, validation failures, domain errors, and cancellation/unexpected cases.
- Use British spelling in descriptions and assertion messages.

**Example**
```csharp
[Test]
public void ToActionResult_WhenValidationError_Returns400()
{
    var result = Result.Failure(ErrorCode.Validation, new[] { "Name is required" });
    var action = result.ToActionResult();
    Assert.That((action as BadRequestObjectResult)?.StatusCode, Is.EqualTo(400));
}
```

**Use this prompt**
> Generate NUnit tests that cover API status mappings, application happy/error paths, and infrastructure adapter error translation.
