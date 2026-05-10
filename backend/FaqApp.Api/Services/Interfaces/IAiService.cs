using FaqApp.Api.Dtos.Ai;

namespace FaqApp.Api.Services.Interfaces;

public interface IAiService
{
    Task<AiSearchResponse> SearchAsync(AiSearchRequest request);
}