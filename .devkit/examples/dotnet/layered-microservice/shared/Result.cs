using System;
using System.Collections.Generic;
using System.Linq;

namespace LayeredMicroservice.Shared;

public class Result
{
    protected Result(bool isSuccess, ErrorCode? code, IReadOnlyCollection<string> errors)
    {
        IsSuccess = isSuccess;
        Code = code;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public ErrorCode? Code { get; }
    public IReadOnlyCollection<string> Errors { get; }

    public static Result Success() => new(true, null, Array.Empty<string>());

    public static Result Failure(ErrorCode code, IEnumerable<string> errors) =>
        new(false, code, Materialise(errors));

    protected static IReadOnlyCollection<string> Materialise(IEnumerable<string> errors) =>
        errors as IReadOnlyCollection<string> ?? errors.ToArray();
}

public class Result<T> : Result
{
    private Result(T? value, bool isSuccess, ErrorCode? code, IReadOnlyCollection<string> errors)
        : base(isSuccess, code, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value, true, null, Array.Empty<string>());

    public static Result<T> Failure(ErrorCode code, IEnumerable<string> errors) =>
        new(default, false, code, Materialise(errors));
}
