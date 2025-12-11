namespace VineyardApi.Models
{
    public enum ErrorCode
    {
        None = 0,
        Validation = 1,
        NotFound = 2,
        Unauthorized = 3,
        Forbidden = 4,
        Conflict = 5,
        Unknown = 6,
        Unexpected = Unknown,
        BadRequest = 7
    }

    public record ValidationError(string Field, string Message);

    public class Result
    {
        public bool Success { get; init; }
        public bool IsSuccess => Success;
        public bool IsFailure => !Success;
        public ErrorCode ErrorCode { get; init; }
        // Alias for backwards compatibility with older tests/usages.
        public ErrorCode Error => ErrorCode;
        public ErrorCode? Code => Success ? null : ErrorCode;
        public string? Message { get; init; }
        public string? ErrorMessage => Message;
        public IReadOnlyCollection<ValidationError> ValidationErrors { get; init; } = Array.Empty<ValidationError>();

        public static Result Ok() => new() { Success = true, ErrorCode = ErrorCode.None };

        public static Result Failure(ErrorCode errorCode, string? message = null, IEnumerable<ValidationError>? validationErrors = null)
        {
            return new Result
            {
                Success = false,
                ErrorCode = errorCode,
                Message = message,
                ValidationErrors = validationErrors?.ToArray() ?? Array.Empty<ValidationError>()
            };
        }
    }

    public class Result<T> : Result
    {
        public T? Value { get; init; }

        public static Result<T> Ok(T value) => new() { Success = true, ErrorCode = ErrorCode.None, Value = value };

        public new static Result<T> Failure(ErrorCode errorCode, string? message = null, IEnumerable<ValidationError>? validationErrors = null)
        {
            return new Result<T>
            {
                Success = false,
                ErrorCode = errorCode,
                Message = message,
                ValidationErrors = validationErrors?.ToArray() ?? Array.Empty<ValidationError>()
            };
        }
    }
}
