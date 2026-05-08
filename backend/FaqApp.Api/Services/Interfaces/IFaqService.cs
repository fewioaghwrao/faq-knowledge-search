using FaqApp.Api.Dtos.Faqs;

namespace FaqApp.Api.Services.Interfaces;

public interface IFaqService
{
    Task<List<FaqListItemDto>> SearchAsync(FaqSearchQuery query);

    Task<FaqListItemDto?> GetByIdAsync(int id);

    Task<FaqListItemDto> CreateAsync(FaqCreateRequest request);

    Task<FaqListItemDto?> UpdateAsync(int id, FaqUpdateRequest request);

    Task<bool> DeleteAsync(int id);
}