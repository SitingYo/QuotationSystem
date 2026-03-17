using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuotationSystem.Models
{
    /// <summary>
    /// 報價單主檔
    /// </summary>
    public class QuotationHeader
    {
        /// <summary>
        /// 系統內部唯一識別碼 (Auto Increment)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 報價單號 (由系統自動生成，格式如：QTN-20260318-01)
        /// </summary>
        public string? QuotationNumber { get; set; }

        /// <summary>
        /// 客戶名稱
        /// </summary>
        [Required]
        public string ClientName { get; set; } = string.Empty;

        /// <summary>
        /// 工程或專案名稱
        /// </summary>
        [Required]
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// 報價日期
        /// </summary>
        public DateTime QuotationDate { get; set; }

        /// <summary>
        /// 未稅總金額 (由各明細小計加總)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalExclTax { get; set; }

        /// <summary>
        /// 營業稅額 (5%，採四捨五入計算)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// 含稅總金額 (未稅金額 + 稅額)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalInclTax { get; set; }

        /// <summary>
        /// 報價單明細項目清單
        /// </summary>
        public List<QuotationDetail> Details { get; set; } = new();
    }

    /// <summary>
    /// 報價單明細檔
    /// </summary>
    public class QuotationDetail
    {
        /// <summary>
        /// 明細內部唯一識別碼
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 項目序號 (1, 2, 3...)
        /// </summary>
        public int ItemNo { get; set; }

        /// <summary>
        /// 所屬主檔之識別碼 (Foreign Key)
        /// </summary>
        public int QuotationHeaderId { get; set; }

        /// <summary>
        /// 項目說明或規格描述
        /// </summary>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 單價
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 數量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 項目小計 (單價 * 數量)
        /// </summary>
        [NotMapped] // 標記為不進入資料庫實體欄位，僅供程式運算
        public decimal SubTotal => UnitPrice * Quantity;
    }
}