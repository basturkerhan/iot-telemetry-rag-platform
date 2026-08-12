using IotPipeline.Platform.Common.Interfaces;
using SmartComponents.LocalEmbeddings;

namespace IotPipeline.Platform.Infrastructure.Services;

public class LocalEmbeddingService : IEmbeddingService
{
    private readonly LocalEmbedder _embedder;

    public LocalEmbeddingService()
    {
        _embedder = new LocalEmbedder();
    }

    public float[] GetEmbedding(string text)
    {
        var vector = _embedder.Embed(text);
        return vector.Values.ToArray();
    }

    public Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var result = GetEmbedding(text);
        return Task.FromResult(result);
    }
}