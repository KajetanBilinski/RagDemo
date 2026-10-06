using Microsoft.Extensions.Options;
using RagApi.Options;

namespace RagApi.Services;

public class RagService
{
    private readonly OllamaEmbeddingService _embeddingService;
    private readonly QdrantService _qdrantService;
    private readonly OllamaChatService _chatService;
    private readonly RagOptions _options;

    public RagService(
        OllamaEmbeddingService embeddingService,
        QdrantService qdrantService,
        OllamaChatService chatService,
        IOptions<RagOptions> options)
    {
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
        _chatService = chatService;
        _options = options.Value;
    }

    public async Task<RagResult> AskAsync(string question)
    {
        var questionEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(question);

        var searchResults =
            await _qdrantService.SearchAsync(
                questionEmbedding,
                _options.SearchLimit,
                _options.ScoreThreshold);

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

                FileName = x.Payload.TryGetValue(
                    "fileName",
                    out var fileName)
                        ? fileName.StringValue
                        : string.Empty,

                ChunkIndex = x.Payload.TryGetValue(
                    "chunkIndex",
                    out var chunkIndex)
                        ? (int)chunkIndex.IntegerValue
                        : 0,

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
    public string FileName { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public string Text { get; set; } = string.Empty;

    public float Score { get; set; }
}