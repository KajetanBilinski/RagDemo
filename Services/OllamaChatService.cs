namespace RagApi.Services;

public class OllamaChatService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OllamaChatService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GenerateAnswerAsync(
        string question,
        string context)
    {
        var model = _configuration["Ollama:ChatModel"]
                    ?? throw new InvalidOperationException(
                        "Ollama chat model is not configured.");

        var request = new
        {
            model,
            stream = false,

            messages = new[]
            {
                new
                {
                    role = "system",
                    content = """
                              You are a RAG assistant.

                              Answer the user's question only using the provided context.

                              If the answer cannot be found in the context,
                              say that you do not know based on the provided documents.

                              Do not invent information.
                              """
                },

                new
                {
                    role = "user",
                    content = $"""
                               CONTEXT:

                               {context}

                               QUESTION:

                               {question}
                               """
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/api/chat",
            request);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OllamaChatResponse>();

        return result?.Message?.Content
               ?? throw new InvalidOperationException(
                   "Ollama did not return an answer.");
    }
}

public class OllamaChatResponse
{
    public OllamaChatMessage? Message { get; set; }
}

public class OllamaChatMessage
{
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}