using FaqApp.Api.Dtos.Ai;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FaqApp.Api.Controller;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] AiSearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _aiService.SearchAsync(request);

        return Ok(result);
    }
}