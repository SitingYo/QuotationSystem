using Microsoft.EntityFrameworkCore;
using QuotationSystem.Data;
using QuotationSystem.Models;

namespace QuotationSystem.Services
{
    /// <summary>
    /// 報價單邏輯服務實作
    /// </summary>
    public class QuotationService : IQuotationService
    {
        private readonly AppDbContext _db;

        public QuotationService(AppDbContext db) => _db = db;

        /// <summary>
        /// 新增報價單並自動計算金額與單號
        /// </summary>
        /// <param name="quotation">前端傳入的報價單資料</param>
        public async Task CreateQuotationAsync(QuotationHeader quotation)
        {
            // 1. 金額運算：計算未稅總和、5%稅額與含稅總計
            quotation.TotalExclTax = quotation.Details.Sum(d => d.UnitPrice * d.Quantity);
            quotation.TaxAmount = Math.Round(quotation.TotalExclTax * 0.05m, 0, MidpointRounding.AwayFromZero);
            quotation.TotalInclTax = quotation.TotalExclTax + quotation.TaxAmount;

            // 2. 報價日期
            quotation.QuotationDate = DateTime.Now;

            // 3. 自動流水號邏輯：格式 QTN-yyyyMMxx (如 QTN-20260301)
            if (string.IsNullOrEmpty(quotation.QuotationNumber))
            {
                string prefix = $"QTN-{DateTime.Now:yyyyMM}";

                // 查詢該月份最大的單號
                var lastQuotation = await _db.QuotationHeaders
                    .Where(q => q.QuotationNumber != null && q.QuotationNumber.StartsWith(prefix))
                    .OrderByDescending(q => q.QuotationNumber)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastQuotation != null && lastQuotation.QuotationNumber != null)
                {
                    // 解析單號最後兩碼
                    string lastTwoDigits = lastQuotation.QuotationNumber.Substring(lastQuotation.QuotationNumber.Length - 2);
                    if (int.TryParse(lastTwoDigits, out int lastId))
                    {
                        nextNumber = lastId + 1;
                    }
                }

                // 格式化單號並填回物件
                quotation.QuotationNumber = $"{prefix}{nextNumber:D2}";
            }

            // 4. 設定明細項次 (ItemNo)
            for (int i = 0; i < quotation.Details.Count; i++)
            {
                quotation.Details[i].ItemNo = i + 1;
            }

            _db.QuotationHeaders.Add(quotation);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 修改報價單
        /// </summary>
        /// <param name="quotation">包含修改內容的報價單物件</param>
        public async Task UpdateQuotationAsync(QuotationHeader quotation)
        {
            // 1. 抓取包含明細的原始資料
            var existingHeader = await _db.QuotationHeaders
                .Include(q => q.Details)
                .FirstOrDefaultAsync(q => q.QuotationNumber == quotation.QuotationNumber);

            if (existingHeader == null) throw new Exception("找不到該報價單，無法修改");

            // 2. 更新主檔描述欄位
            existingHeader.ClientName = quotation.ClientName;
            existingHeader.ProjectName = quotation.ProjectName;

            // 3. 明細維護：採用先刪除後新增的方式確保資料同步
            _db.QuotationDetails.RemoveRange(existingHeader.Details);
            existingHeader.Details = quotation.Details;

            // 4. 重新計算財務金額 (避免前端計算錯誤)
            existingHeader.TotalExclTax = existingHeader.Details.Sum(d => d.UnitPrice * d.Quantity);
            existingHeader.TaxAmount = Math.Round(existingHeader.TotalExclTax * 0.05m, 0, MidpointRounding.AwayFromZero);
            existingHeader.TotalInclTax = existingHeader.TotalExclTax + existingHeader.TaxAmount;

            // 5. 重新分配明細項次 (1, 2, 3...)
            for (int i = 0; i < existingHeader.Details.Count; i++)
            {
                existingHeader.Details[i].ItemNo = i + 1;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<QuotationHeader>> GetAllQuotationsAsync()
        {
            return await _db.QuotationHeaders
                .Include(q => q.Details)
                .OrderByDescending(q => q.QuotationDate)
                .ToListAsync();
        }
    }
}