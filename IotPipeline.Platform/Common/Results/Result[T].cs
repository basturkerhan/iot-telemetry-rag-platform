using System.Text.Json.Serialization;

namespace IotPipeline.Platform.Common.Results;

public record Result<TValue>(
    [property: JsonIgnore] bool IsSuccess,
    TValue? Value,
    List<string>? Errors
)
{
    public static Result<TValue> Success() => new(
        IsSuccess: true,
        Errors: null,
        Value: default
    );
    public static Result<TValue> Success(TValue value) => new(
        IsSuccess: true,
        Errors: null,
        Value: value
    );
    public static Result<TValue> Failure(List<string> errors) => new(
        IsSuccess: false,
        Errors: errors,
        Value: default
    );
    public static Result<TValue> Failure(string error) => new(
        IsSuccess: false,
        Errors: [error],
        Value: default
    );
}
