namespace IotPipeline.Platform.Common.Interfaces;

public interface IRagService
{
    Task<string> AskQuestionWithContextAsync(
        string question,
        string context,
        CancellationToken cancellationToken = default);
}
