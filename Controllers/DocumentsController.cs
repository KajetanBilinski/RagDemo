using Microsoft.AspNetCore.Mvc;
using RagApi.Models;
using RagApi.Services;

namespace RagApi.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly OllamaEmbeddingService _embeddingService;
    private readonly QdrantService _qdrantService;
    private readonly ChunkingService _chunkingService;

    public DocumentsController(
        OllamaEmbeddingService embeddingService,
        QdrantService qdrantService,
        ChunkingService chunkingService)
    {
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
        _chunkingService = chunkingService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
    IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        if (Path.GetExtension(file.FileName)
            .ToLowerInvariant() != ".txt")
        {
            return BadRequest(
                "Currently only .txt files are supported.");
        }

        using var reader =
            new StreamReader(file.OpenReadStream());

        var text = await reader.ReadToEndAsync();

        var chunks = _chunkingService.Split(text);

        var documentId = Guid.NewGuid();

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];

            var embedding =
                await _embeddingService
                    .GenerateEmbeddingAsync(chunk);

            await _qdrantService.StoreChunkAsync(
                documentId,
                file.FileName,
                i,
                chunk,
                embedding);
        }

        return Ok(new
        {
            documentId,
            fileName = file.FileName,
            length = text.Length,
            chunks = chunks.Count
        });
    }

    [HttpPost]
    public async Task<IActionResult> Add(EmbeddingRequest request)
    {
        var embedding =
            await _embeddingService.GenerateEmbeddingAsync(request.Text);

        await _qdrantService.StoreAsync(
            request.Text,
            embedding);

        return Ok(new
        {
            stored = true,
            dimensions = embedding.Length
        });
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search(
    EmbeddingRequest request)
    {
        var embedding =
            await _embeddingService.GenerateEmbeddingAsync(request.Text);

        var results =
            await _qdrantService.SearchAsync(
                embedding);

        var response = results.Select(x => new
        {
            score = x.Score,
            text = x.Payload.TryGetValue("text", out var value)
                ? value.StringValue
                : null
        });

        return Ok(response);
    }
}