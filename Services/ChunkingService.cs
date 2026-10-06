using Microsoft.Extensions.Options;
using RagApi.Options;

namespace RagApi.Services;

public class ChunkingService
{
    private readonly RagOptions _options;

    public ChunkingService(
        IOptions<RagOptions> options)
    {
        _options = options.Value;
    }

    public List<string> Split(string text)
    {
        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        var chunkSize = _options.ChunkSize;
        var overlap = _options.ChunkOverlap;

        var start = 0;

        while (start < text.Length)
        {
            var length = Math.Min(
                chunkSize,
                text.Length - start);

            var chunk = text
                .Substring(start, length)
                .Trim();

            if (!string.IsNullOrWhiteSpace(chunk))
                chunks.Add(chunk);

            if (start + length >= text.Length)
                break;

            start += chunkSize - overlap;
        }

        return chunks;
    }
}