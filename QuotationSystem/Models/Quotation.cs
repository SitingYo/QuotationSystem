namespace QuotationSystem.Models
{
    // 報價單主檔
    public class QuotationHeader
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } // 報價單號
        public string ClientName { get; set; }      // 客戶名稱
        public string ProjectName { get; set; }     // 工程名稱
        public DateTime QuotationDate { get; set; }  // 報價日期

        // 金額欄位 (由後端計算)
        public decimal TotalExclTax { get; set; }   // 未稅金額
        public decimal TaxAmount { get; set; }      // 稅額
        public decimal TotalInclTax { get; set; }   // 含稅金額

        // 一對多關聯：一個主檔有多筆明細
        public List<QuotationDetail> Details { get; set; } = new();
    }

    // 報價單明細檔
    public class QuotationDetail
    {
        public int Id { get; set; }
        public int QuotationHeaderId { get; set; }  // 外鍵
        public string Description { get; set; }     // 項目說明
        public decimal UnitPrice { get; set; }      // 單價
        public int Quantity { get; set; }           // 數量

        // 小計 (唯讀屬性，不進資料庫也可，或由程式計算)
        public decimal SubTotal => UnitPrice * Quantity;
    }
}