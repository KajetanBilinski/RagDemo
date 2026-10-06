using Microsoft.AspNetCore.Mvc;
using RagApi.Models;
using RagApi.Services;

namespace RagApi.Controllers;

[ApiController]
[Route("api/rag")]
public class RagController : ControllerBase
{
    private readonly RagService _ragService;

    public RagController(RagService ragService)
    {
        _ragService = ragService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
        RagQuestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty.");
        }

        var result =
            await _ragService.AskAsync(request.Question);

        return Ok(result);
    }
}