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

    public DocumentsController(
        OllamaEmbeddingService embeddingService,
        QdrantService qdrantService)
    {
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
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