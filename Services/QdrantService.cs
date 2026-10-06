using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace RagApi.Services;

public class QdrantService
{
    private readonly QdrantClient _client;
    private readonly string _collectionName;

    public QdrantService(IConfiguration configuration)
    {
        var host = configuration["Qdrant:Host"] ?? "localhost";
        var port = int.Parse(configuration["Qdrant:Port"] ?? "6334");

        _collectionName =
            configuration["Qdrant:CollectionName"]
            ?? "documents";

        _client = new QdrantClient(host, port);
    }

    public async Task EnsureCollectionExistsAsync()
    {
        var exists =
            await _client.CollectionExistsAsync(_collectionName);

        if (exists)
            return;

        await _client.CreateCollectionAsync(
            _collectionName,
            new VectorParams
            {
                Size = 768,
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
            _collectionName,
            [point]);
    }




    public async Task<IReadOnlyList<ScoredPoint>> SearchAsync(
        float[] embedding,
        ulong limit = 3,
        float scoreThreshold = 0.5f)
    {
        await EnsureCollectionExistsAsync();

        var results = await _client.QueryAsync(
            _collectionName,
            embedding,
            limit: limit,
            scoreThreshold: scoreThreshold);

        return results;
    }
}