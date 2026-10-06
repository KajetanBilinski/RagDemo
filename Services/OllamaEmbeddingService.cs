namespace RagApi.Services;

using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RagApi.Options;


public class OllamaEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaEmbeddingService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var request = new
        {
            model = _options.EmbeddingModel,
            input = text
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/embed",
            request);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OllamaEmbeddingResponse>();

        if (result?.Embeddings is null ||
            result.Embeddings.Length == 0)
        {
            throw new InvalidOperationException(
                "Ollama did not return an embedding.");
        }

        return result.Embeddings[0];
    }
}

public class OllamaEmbeddingResponse
{
    public float[][] Embeddings { get; set; } = [];
}