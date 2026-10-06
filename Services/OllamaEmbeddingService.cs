namespace RagApi.Services;

public class OllamaEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OllamaEmbeddingService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var model = _configuration["Ollama:EmbeddingModel"];

        var request = new
        {
            model,
            input = text
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/embed",
            request);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();

        if (result?.Embeddings is null || result.Embeddings.Length == 0)
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