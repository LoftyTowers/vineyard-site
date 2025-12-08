namespace LayeredMicroservice.Shared;

public enum ErrorCode
{
    Validation = 400,
    Domain = 422,
    Cancelled = 499,
    Unexpected = 500
}

public static class ErrorCodeExtensions
{
    public static string GetTitle(this ErrorCode code) => code switch
    {
        ErrorCode.Validation => "Request validation failed",
        ErrorCode.Domain => "Business rule violated",
        ErrorCode.Cancelled => "Request was cancelled",
        ErrorCode.Unexpected => "Unexpected error occurred",
        _ => "Unhandled error"
    };
}
