// Services/QuotationService.cs
using QuotationSystem.Data;
using QuotationSystem.Models;

namespace QuotationSystem.Services
{
    public class QuotationService
    {
        private readonly AppDbContext _db;
        public QuotationService(AppDbContext db) => _db = db;

        public async Task CreateQuotationAsync(QuotationHeader quotation)
        {
            // 1. 計算明細總和 (未稅)
            quotation.TotalExclTax = quotation.Details.Sum(d => d.UnitPrice * d.Quantity);

            // 2. 計算 5% 稅額 (四捨五入至整數)
            quotation.TaxAmount = Math.Round(quotation.TotalExclTax * 0.05m, 0, MidpointRounding.AwayFromZero);

            // 3. 計算含稅總額
            quotation.TotalInclTax = quotation.TotalExclTax + quotation.TaxAmount;

            // 4. 設定建立日期與單號 (範例單號)
            quotation.QuotationDate = DateTime.Now;
            if (string.IsNullOrEmpty(quotation.QuotationNumber))
                quotation.QuotationNumber = $"QTN-{DateTime.Now:yyyyMMddHHmmss}";

            _db.QuotationHeaders.Add(quotation);
            await _db.SaveChangesAsync();
        }
    }
}