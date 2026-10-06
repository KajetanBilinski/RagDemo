using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using RagApi.Options;

namespace RagApi.Services;

public class QdrantService
{
    private readonly QdrantClient _client;
    private readonly QdrantOptions _options;

    public QdrantService(
        IOptions<QdrantOptions> options)
    {
        _options = options.Value;

        _client = new QdrantClient(
            _options.Host,
            _options.Port);
    }

    public async Task EnsureCollectionExistsAsync()
    {
        var exists =
            await _client.CollectionExistsAsync(
                _options.CollectionName);

        if (exists)
            return;

        await _client.CreateCollectionAsync(
            _options.CollectionName,
            new VectorParams
            {
                Size = _options.VectorSize,
                Distance = Distance.Cosine
            });
    }

    public async Task StoreAsync(
        string text,
        float[] embedding)
    {
        await EnsureCollectionExistsAsync();

        var point = new PointStruct
        {
            Id = Guid.NewGuid(),
            Vectors = embedding
        };

        point.Payload["text"] = text;

        await _client.UpsertAsync(
            _options.CollectionName,
            [point]);
    }

    public async Task StoreChunkAsync(
    Guid documentId,
    string fileName,
    int chunkIndex,
    string text,
    float[] embedding)
    {
        await EnsureCollectionExistsAsync();

        var point = new PointStruct
        {
            Id = Guid.NewGuid(),
            Vectors = embedding
        };

        point.Payload["documentId"] = documentId.ToString();
        point.Payload["fileName"] = fileName;
        point.Payload["chunkIndex"] = chunkIndex;
        point.Payload["text"] = text;

        await _client.UpsertAsync(
            _options.CollectionName,
            new[] { point });
    }


    public async Task<IReadOnlyList<ScoredPoint>> SearchAsync(
        float[] embedding,
        ulong limit = 3,
        float scoreThreshold = 0.5f)
    {
        await EnsureCollectionExistsAsync();

        var results = await _client.QueryAsync(
            _options.CollectionName,
            embedding,
            limit: limit,
            scoreThreshold: scoreThreshold);

        return results;
    }
}