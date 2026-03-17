using QuotationSystem.Models;

namespace QuotationSystem.Services
{
    public interface IQuotationService
    {
        Task CreateQuotationAsync(QuotationHeader quotation);
        Task UpdateQuotationAsync(QuotationHeader quotation);
        Task<List<QuotationHeader>> GetAllQuotationsAsync();

    }
}