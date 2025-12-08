namespace VineyardApi.Services
{
    public enum ErrorCode
    {
        None,
        NotFound,
        Domain,
        Unauthorized,
        Unexpected
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public ErrorCode Error { get; }
        public string? Message { get; }

        protected Result(bool isSuccess, ErrorCode error = ErrorCode.None, string? message = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            Message = message;
        }

        public static Result Success() => new(true);

        public static Result Failure(ErrorCode error, string? message = null) => new(false, error, message);
    }

    public sealed class Result<T> : Result
    {
        public T? Value { get; }

        private Result(bool isSuccess, T? value, ErrorCode error, string? message)
            : base(isSuccess, error, message)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, value, ErrorCode.None, null);

        public static Result<T> Failure(ErrorCode error, string? message = null) => new(false, default, error, message);
    }
}
