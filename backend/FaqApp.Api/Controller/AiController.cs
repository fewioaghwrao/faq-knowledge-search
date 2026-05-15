using FaqApp.Api.Dtos.Ai;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FaqApp.Api.Controller;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly IAiSearchHistoryService _aiSearchHistoryService;

    public AiController(
        IAiService aiService,
        IAiSearchHistoryService aiSearchHistoryService)
    {
        _aiService = aiService;
        _aiSearchHistoryService = aiSearchHistoryService;
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

    [HttpGet("histories")]
    public async Task<IActionResult> GetHistories([FromQuery] AiSearchHistoryQuery query)
    {
        var result = await _aiSearchHistoryService.GetListAsync(query);

        return Ok(result);
    }

    [HttpGet("histories/{id:int}")]
    public async Task<IActionResult> GetHistoryDetail(int id)
    {
        var result = await _aiSearchHistoryService.GetDetailAsync(id);

        if (result is null)
        {
            return NotFound(new
            {
                message = "指定されたAI検索履歴は存在しません。"
            });
        }

        return Ok(result);
    }

    [HttpPost("histories/{id:int}/feedback")]
    public async Task<IActionResult> UpdateFeedback(
        int id,
        [FromBody] AiSearchFeedbackRequestDto request)
    {
        try
        {
            await _aiSearchHistoryService.UpdateFeedbackAsync(id, request.IsHelpful);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}