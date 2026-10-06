using Microsoft.AspNetCore.Mvc;
using RagApi.Models;
using RagApi.Services;

namespace RagApi.Controllers;

[ApiController]
[Route("api/embeddings")]
public class EmbeddingsController : ControllerBase
{
    private readonly OllamaEmbeddingService _embeddingService;

    public EmbeddingsController(OllamaEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    [HttpPost]
    public async Task<IActionResult> Generate(EmbeddingRequest request)
    {
        var embedding =
            await _embeddingService.GenerateEmbeddingAsync(request.Text);

        return Ok(new
        {
            dimensions = embedding.Length,
            embedding
        });
    }
}