using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuotationSystem.Data;
using QuotationSystem.Models;
using QuotationSystem.Services;

namespace QuotationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly QuotationService _service;

        public QuotationsController(AppDbContext context, QuotationService service)
        {
            _context = context;
            _service = service;
        }

        // 1. POST: api/Quotations (建立報價單與明細)
        [HttpPost]
        public async Task<ActionResult<QuotationHeader>> CreateQuotation(QuotationHeader quotation)
        {
            try
            {
                await _service.CreateQuotationAsync(quotation);
                return CreatedAtAction(nameof(GetQuotation), new { id = quotation.Id }, quotation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. GET: api/Quotations/5 (取得單一報價單，含明細)
        [HttpGet("{id}")]
        public async Task<ActionResult<QuotationHeader>> GetQuotation(int id)
        {
            // 使用 Include 語法一次將關聯的明細 (Details) 撈出來
            var quotation = await _context.QuotationHeaders
                .Include(q => q.Details)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quotation == null) return NotFound();

            return quotation;
        }

        // 3. GET: api/Quotations (取得清單，不含明細以節省流量)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuotationHeader>>> GetQuotations()
        {
            return await _context.QuotationHeaders.ToListAsync();
        }
    }
}