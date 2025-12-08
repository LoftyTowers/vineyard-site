# .NET Libraries (Pinned)

- Logging: ILogger<T> with Serilog (structured templates).
- Testing: NUnit + Moq + FluentAssertions.
- Validation: FluentValidation.
- Resilience: Polly where idempotent (timeouts, retry/backoff, circuit breaker).
- HTTP: HttpClientFactory; no `new HttpClient()`.
- Config: Options pattern; validate on start; no secrets in code or logs.
