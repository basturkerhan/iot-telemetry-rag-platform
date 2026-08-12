namespace IotPipeline.Platform.Common.Results
{
    public interface IResult
    {
        bool IsSuccess { get; }
        IReadOnlyList<string>? Errors { get; }
        object? GetData();
        string? ToStringData();
    }
}
