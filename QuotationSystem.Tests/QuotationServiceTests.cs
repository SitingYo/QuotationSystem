using Microsoft.EntityFrameworkCore;
using QuotationSystem.Data;
using QuotationSystem.Models;
using QuotationSystem.Services;
using Xunit;
using FluentAssertions;

namespace QuotationSystem.Tests
{
    public class QuotationServiceTests
    {
        // 建立一個乾淨的記憶體資料庫環境
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // 每次測試使用唯一名稱確保資料隔離
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        [Trait("Category", "Financial")]
        public async Task CreateQuotationAsync_ShouldCalculateCorrectTotalAndTax()
        {
            // Arrange (準備)
            var db = GetDbContext();
            var service = new QuotationService(db);

            var quotation = new QuotationHeader
            {
                ClientName = "測試客戶",
                ProjectName = "單元測試專案",
                Details = new List<QuotationDetail>
                {
                    new QuotationDetail { Description = "項目 A", UnitPrice = 1000, Quantity = 2 }, // 2000
                    new QuotationDetail { Description = "項目 B", UnitPrice = 555, Quantity = 1 }    // 555
                }
            };

            // Act (執行)
            await service.CreateQuotationAsync(quotation);

            // Assert (驗證)
            // 未稅總計：2000 + 555 = 2555
            // 稅額：2555 * 0.05 = 127.75 -> 四捨五入應為 128
            // 含稅總計：2555 + 128 = 2683
            quotation.TotalExclTax.Should().Be(2555);
            quotation.TaxAmount.Should().Be(128);
            quotation.TotalInclTax.Should().Be(2683);
        }

        [Fact]
        [Trait("Category", "BusinessLogic")]
        public async Task CreateQuotationAsync_ShouldGenerateFormattedNumber()
        {
            // Arrange
            var db = GetDbContext();
            var service = new QuotationService(db);
            var quotation = new QuotationHeader { ClientName = "客戶", ProjectName = "專案" };

            // Act
            await service.CreateQuotationAsync(quotation);

            // Assert
            // 驗證單號格式是否為 QTN-yyyyMMxx
            string prefix = $"QTN-{DateTime.Now:yyyyMM}";
            quotation.QuotationNumber.Should().StartWith(prefix);
            quotation.QuotationNumber?.Length.Should().Be(prefix.Length + 2);
        }

        [Fact]
        [Trait("Category", "Sequence")]
        public async Task CreateQuotationAsync_ShouldIncrementSequenceNumber()
        {
            // Arrange
            var db = GetDbContext();
            var service = new QuotationService(db);

            var q1 = new QuotationHeader { ClientName = "客戶1", ProjectName = "專案1" };
            var q2 = new QuotationHeader { ClientName = "客戶2", ProjectName = "專案2" };

            // Act
            await service.CreateQuotationAsync(q1);
            await service.CreateQuotationAsync(q2);

            // Assert
            // 驗證第二張單據的流水號是否比第一張大 1
            int n1 = int.Parse(q1.QuotationNumber!.Substring(q1.QuotationNumber.Length - 2));
            int n2 = int.Parse(q2.QuotationNumber!.Substring(q2.QuotationNumber.Length - 2));

            n2.Should().Be(n1 + 1);
        }

        [Fact]
        public async Task UpdateQuotationAsync_ShouldThrowException_WhenQuotationNotFound()
        {
            // Arrange
            var db = GetDbContext();
            var service = new QuotationService(db);
            var nonExistentQuotation = new QuotationHeader { QuotationNumber = "NON-EXIST" };

            // Act & Assert
            // 驗證當單號不存在時，是否真的會拋出 Exception
            await Assert.ThrowsAsync<Exception>(() => service.UpdateQuotationAsync(nonExistentQuotation));
        }
    }
}