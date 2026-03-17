using Microsoft.EntityFrameworkCore;
using QuotationSystem.Models;

namespace QuotationSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 報價單主檔
        public DbSet<QuotationHeader> QuotationHeaders { get; set; }

        // 報價單明細檔
        public DbSet<QuotationDetail> QuotationDetails { get; set; }
    }
}