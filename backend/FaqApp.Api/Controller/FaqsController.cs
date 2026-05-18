using FaqApp.Api.Dtos.Faqs;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaqApp.Api.Controller;

[ApiController]
[Route("api/faqs")]
public class FaqsController : ControllerBase
{
    private readonly IFaqService _faqService;

    public FaqsController(IFaqService faqService)
    {
        _faqService = faqService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FaqListItemDto>>> Search([FromQuery] FaqSearchQuery query)
    {
        var result = await _faqService.SearchAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FaqListItemDto>> GetById(int id)
    {
        var result = await _faqService.GetByIdAsync(id);

        if (result is null)
        {
            return NotFound(new { message = "FAQが見つかりません。" });
        }

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<FaqListItemDto>> Create(FaqCreateRequest request)
    {
        try
        {
            var result = await _faqService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FaqListItemDto>> Update(int id, FaqUpdateRequest request)
    {
        try
        {
            var result = await _faqService.UpdateAsync(id, request);

            if (result is null)
            {
                return NotFound(new { message = "FAQが見つかりません。" });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _faqService.DeleteAsync(id);

        if (!result)
        {
            return NotFound(new { message = "FAQが見つかりません。" });
        }

        return NoContent();
    }
}