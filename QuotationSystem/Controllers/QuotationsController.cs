using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuotationSystem.Data;
using QuotationSystem.Models;
using QuotationSystem.Services;

namespace QuotationSystem.Controllers
{
    /// <summary>
    /// 報價單管理介面
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IQuotationService _service;

        public QuotationsController(AppDbContext context, IQuotationService service)
        {
            _context = context;
            _service = service;
        }

        /// <summary>
        /// 建立新的報價單
        /// </summary>
        /// <remarks>
        /// 系統將自動執行：
        /// 1. 生成報價單號 (QTN-yyyyMMdd-XX)
        /// 2. 計算 5% 營業稅
        /// 3. 自動編排明細項次 (ItemNo)
        /// </remarks>
        /// <param name="quotation">報價單內容 (不需包含 Id 與單號)</param>
        /// <response code="201">建立成功，並回傳完整單據內容</response>
        /// <response code="400">建立失敗，通常為輸入格式有誤或邏輯異常</response>
        [HttpPost]
        [ProducesResponseType(typeof(QuotationHeader), 201)]
        public async Task<ActionResult> CreateQuotation(QuotationHeader quotation)
        {
            try
            {
                await _service.CreateQuotationAsync(quotation);

                return CreatedAtAction(
                    nameof(GetQuotation),
                    new { quotationNumber = quotation.QuotationNumber },
                    new { code = 201, message = "建立成功", data = quotation });
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = 400, message = ex.Message });
            }
        }

        /// <summary>
        /// 查詢報價單 (清單或單筆)
        /// </summary>
        /// <param name="quotationNumber">選填。若不傳入則回傳完整清單；若傳入則回傳單筆明細</param>
        /// <response code="200">查詢成功</response>
        /// <response code="404">找不到指定單號</response>
        [HttpGet("{quotationNumber?}")]
        public async Task<ActionResult> GetQuotation(string? quotationNumber = null)
        {
            var query = _context.QuotationHeaders.Include(q => q.Details).AsNoTracking();

            if (!string.IsNullOrEmpty(quotationNumber))
            {
                var quotation = await query.FirstOrDefaultAsync(q => q.QuotationNumber == quotationNumber);
                if (quotation == null)
                    return NotFound(new { code = 404, message = $"找不到單號：{quotationNumber}" });

                return Ok(new { code = 200, message = "查詢成功", data = quotation });
            }

            // 預設依日期降冪排序，讓最新的報價單排在前面
            var list = await query.OrderByDescending(q => q.QuotationDate).ToListAsync();
            return Ok(new { code = 200, message = "讀取清單成功", data = list });
        }

        /// <summary>
        /// 修改現有報價單
        /// </summary>
        /// <remarks>
        /// 系統會替換所有明細內容並重新計算總計金額。
        /// </remarks>
        /// <param name="quotationNumber">欲修改的報價單號</param>
        /// <param name="quotation">更新後的物件內容</param>
        /// <response code="200">修改成功</response>
        /// <response code="400">單號不符或更新邏輯錯誤</response>
        [HttpPut("{quotationNumber}")]
        public async Task<ActionResult> UpdateQuotation(string quotationNumber, QuotationHeader quotation)
        {
            if (quotationNumber != quotation.QuotationNumber)
                return BadRequest(new { code = 400, message = "URL單號與內容不符" });

            try
            {
                await _service.UpdateQuotationAsync(quotation);
                return Ok(new { code = 200, message = "修改成功", data = quotation });
            }
            catch (Exception ex)
            {
                return BadRequest(new { code = 400, message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除報價單
        /// </summary>
        /// <param name="quotationNumber">欲刪除的報價單號</param>
        /// <response code="200">刪除成功</response>
        /// <response code="404">找不到該單號</response>
        [HttpDelete("{quotationNumber}")]
        public async Task<ActionResult> DeleteQuotation(string quotationNumber)
        {
            var quotation = await _context.QuotationHeaders.FirstOrDefaultAsync(q => q.QuotationNumber == quotationNumber);
            if (quotation == null)
                return NotFound(new { code = 404, message = "找不到單據" });

            _context.QuotationHeaders.Remove(quotation);
            await _context.SaveChangesAsync();
            return Ok(new { code = 200, message = "刪除成功" });
        }
    }
}