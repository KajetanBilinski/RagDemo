namespace RagApi.Services;

public class RagService
{
    private readonly OllamaEmbeddingService _embeddingService;
    private readonly QdrantService _qdrantService;
    private readonly OllamaChatService _chatService;

    public RagService(
        OllamaEmbeddingService embeddingService,
        QdrantService qdrantService,
        OllamaChatService chatService)
    {
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
        _chatService = chatService;
    }

    public async Task<RagResult> AskAsync(string question)
    {
        var questionEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(question);

        var searchResults = await _qdrantService.SearchAsync(
            questionEmbedding,
            limit: 3,
            scoreThreshold: 0.5f);

        if (searchResults.Count == 0)
        {
            return new RagResult
            {
                Answer = "I do not know based on the provided documents.",
                Sources = []
            };
        }

        var sources = searchResults
            .Where(x => x.Payload.ContainsKey("text"))
            .Select(x => new RagSource
            {
                Text = x.Payload["text"].StringValue,
                Score = x.Score
            })
            .ToList();

        var context = string.Join(
            "\n\n---\n\n",
            sources.Select(x => x.Text));

        var answer =
            await _chatService.GenerateAnswerAsync(
                question,
                context);

        return new RagResult
        {
            Answer = answer,
            Sources = sources
        };
    }
}

public class RagResult
{
    public string Answer { get; set; } = string.Empty;

    public List<RagSource> Sources { get; set; } = [];
}

public class RagSource
{
    public string Text { get; set; } = string.Empty;

    public float Score { get; set; }
}